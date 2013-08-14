using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sam.XmlDiffPath
{
    internal class DiffgramGenerator
    {
        #region Fields
        private XmlDiff xmlDiff;

        // cached !XmlDiff.IgnoreChildOrder
        private bool isChildOrderSignificant;

        // Descriptors & operation IDs
        private ulong lastOperationID;

        // nodes in the post-order numbering - cached from XmlDiff 
        private XmlDiffNode[] sourceNodes;
        private XmlDiffNode[] targetNodes;

        // current processed nodes
        private int curSourceIndex;
        private int curTargetIndex;

        // processed edit script
        private EditScript editScript;

        // 'prefix' descriptors
        private PrefixChange prefixChangeDescr = null;

        // 'namespace' descriptors
        private NamespaceChange namespaceChangeDescr = null;

        // substitute edit script 
        private PostponedEditScriptInfo postponedEditScript;
        private bool isBuildingAddTree = false;

        // cached DiffgramPosition object
        DiffgramPosition cachedDiffgramPosition = new DiffgramPosition(null);

        // 'move' descriptors
        internal const int MoveHashtableInitialSize = 8;
        internal Hashtable moveDescriptors = new Hashtable(MoveHashtableInitialSize);
        #endregion

        #region Constructors
        internal DiffgramGenerator(XmlDiff xmlDiff)
        {
            Debug.Assert(xmlDiff != null);

            this.xmlDiff = xmlDiff;
            isChildOrderSignificant = !xmlDiff.IgnoreChildOrder;

            lastOperationID = 0;
        }
        #endregion

        #region Internal Methods

        internal Diffgram GenerateEmptyDiffgram()
        {
            return new Diffgram(this.xmlDiff);
        }

        internal Diffgram GenerateFromEditScript(EditScript editScript)
        {
            Debug.Assert(editScript != null);

            Debug.Assert(this.xmlDiff.sourceNodes != null);
            Debug.Assert(this.xmlDiff.targetNodes != null);
            this.sourceNodes = this.xmlDiff.sourceNodes;
            this.targetNodes = this.xmlDiff.targetNodes;

            Diffgram diffgram = new Diffgram(this.xmlDiff);

            // root nodes always match; remove them from the edit script
            EditScriptMatch esm = editScript as EditScriptMatch;
            if (editScript.Operation == EditScriptOperation.Match &&
                 (esm._firstSourceIndex + esm._length == sourceNodes.Length &&
                   esm._firstTargetIndex + esm._length == targetNodes.Length))
            {
                esm._length--;
                if (esm._length == 0)
                    editScript = esm.nextEditScript;
            }
            else
                Debug.Assert(false, "The root nodes does not match!");

            // initialize globals
            this.curSourceIndex = this.sourceNodes.Length - 2;
            this.curTargetIndex = this.targetNodes.Length - 2;
            this.editScript = editScript;

            // generate diffgram
            this.GenerateDiffgramMatch(diffgram, 1, 1);

            // add descriptors
            this.AppendDescriptors(diffgram);

            return diffgram;
        }

        #endregion

        #region Private Methods

        private void AppendDescriptors(Diffgram diffgram)
        {
            IDictionaryEnumerator en = this.moveDescriptors.GetEnumerator();
            while (en.MoveNext())
                diffgram.AddDescriptor(new OperationDescrMove((ulong)en.Value));
            NamespaceChange nsChange = namespaceChangeDescr;
            while (nsChange != null)
            {
                diffgram.AddDescriptor(new OperationDescrNamespaceChange(nsChange));
                nsChange = nsChange._next;
            }

            PrefixChange prefixChange = prefixChangeDescr;
            while (prefixChange != null)
            {
                diffgram.AddDescriptor(new OperationDescrPrefixChange(prefixChange));
                prefixChange = prefixChange._next;
            }
        }

        private void GenerateDiffgramMatch(DiffgramParentOperation parent, int sourceBorderIndex, int targetBorderIndex)
        {
            bool bNeedPosition = false;

            while (this.curSourceIndex >= sourceBorderIndex || this.curTargetIndex >= targetBorderIndex)
            {
                Debug.Assert(this.editScript != null);

                switch (this.editScript.Operation)
                {
                    case EditScriptOperation.Match:
                        OnMatch(parent, bNeedPosition);
                        bNeedPosition = false;
                        break;
                    case EditScriptOperation.Add:
                        bNeedPosition = OnAdd(parent, sourceBorderIndex, targetBorderIndex);
                        break;
                    case EditScriptOperation.Remove:
                        if (this.curSourceIndex < sourceBorderIndex)
                            return;
                        OnRemove(parent);
                        break;
                    case EditScriptOperation.ChangeNode:
                        if (this.curSourceIndex < sourceBorderIndex)
                            return;
                        OnChange(parent);
                        break;
                    case EditScriptOperation.EditScriptPostponed:
                        if (this.curSourceIndex < sourceBorderIndex)
                            return;
                        OnEditScriptPostponed(parent, targetBorderIndex);
                        break;
                    default:
                        Debug.Assert(false, "Invalid edit script operation type in final edit script.");
                        break;
                }
            }
        }

        private void OnMatch(DiffgramParentOperation parent, bool bNeedPosition)
        {
            EditScriptMatch matchOp = this.editScript as EditScriptMatch;

            Debug.Assert(this.curSourceIndex == matchOp._firstSourceIndex + matchOp._length - 1);
            Debug.Assert(this.curTargetIndex == matchOp._firstTargetIndex + matchOp._length - 1);

            // cache
            int endTargetIndex = matchOp._firstTargetIndex + matchOp._length - 1;
            int endSourceIndex = matchOp._firstSourceIndex + matchOp._length - 1;
            XmlDiffNode targetRoot = this.targetNodes[endTargetIndex];
            XmlDiffNode sourceRoot = this.sourceNodes[endSourceIndex];

            // a subtree or leaf node matches
            if (matchOp._firstTargetIndex <= targetRoot.Left &&
                 matchOp._firstSourceIndex <= sourceRoot.Left)
            {
                if (isBuildingAddTree)
                {
                    Debug.Assert(!bNeedPosition);

                    ulong opid = GenerateOperationID(XmlDiffDescriptorType.Move);

                    // output <add match=" "> to diffgram and "remove" to substitute script
                    parent.InsertAtBeginning(new DiffgramCopy(sourceRoot, true, opid));

                    // add 'remove' operation to postponed operations
                    PostponedRemoveSubtrees(sourceRoot, opid,
                        //AddToPosponedOperations( new DiffgramRemoveSubtrees( sourceRoot, opid ), 
                                            sourceRoot.Left,
                                            endSourceIndex);
                }
                else
                {
                    // matched element -> check attributes if they really match (hash values of attributes matches)
                    if (sourceRoot.NodeType == XmlDiffNodeType.Element)
                    {
                        DiffgramPosition diffPos = cachedDiffgramPosition;
                        diffPos.sourceNode = sourceRoot;

                        GenerateChangeDiffgramForAttributes(diffPos, (XmlDiffElement)sourceRoot, (XmlDiffElement)targetRoot);

                        if (diffPos._firstChildOp != null || bNeedPosition)
                        {
                            parent.InsertAtBeginning(diffPos);
                            cachedDiffgramPosition = new DiffgramPosition(null);
                            bNeedPosition = false;
                        }
                    }
                    // otherwise output <node> - only if we need the position (<=> preceding operation was 'add')
                    else
                    {
                        if (bNeedPosition)
                        {
                            parent.InsertAtBeginning(new DiffgramPosition(sourceRoot));
                            bNeedPosition = false;
                        }
                        // XML declaration, DTD
                        else if (!this.isChildOrderSignificant && (int)sourceRoot.NodeType < 0)
                        {
                            DiffgramOperation op = parent._firstChildOp;
                            if (op is DiffgramAddNode || op is DiffgramAddSubtrees || op is DiffgramCopy)
                            {
                                parent.InsertAtBeginning(new DiffgramPosition(sourceRoot));
                            }
                        }
                    }
                }

                // adjust current position
                this.curSourceIndex = sourceRoot.Left - 1;
                this.curTargetIndex = targetRoot.Left - 1;

                // adjust boundaries in the edit script or move to next edit script operation
                matchOp._length -= endTargetIndex - targetRoot.Left + 1;
                if (matchOp._length <= 0)
                    this.editScript = this.editScript.nextEditScript;
            }
            // single but non-leaf node matches (-> recursively generate the diffgram subtree)
            else
            {
                // adjust current position
                this.curSourceIndex--;
                this.curTargetIndex--;

                // adjust boundaries in the edit script or move to next edit script operation
                matchOp._length--;
                if (matchOp._length <= 0)
                    this.editScript = this.editScript.nextEditScript;

                DiffgramParentOperation diffgramNode;
                if (isBuildingAddTree)
                {
                    Debug.Assert(!bNeedPosition);

                    ulong opid = GenerateOperationID(XmlDiffDescriptorType.Move);
                    bool bCopySubtree = sourceRoot.NodeType != XmlDiffNodeType.Element;

                    // output <add match=".." subtree="no">
                    diffgramNode = new DiffgramCopy(sourceRoot, bCopySubtree, opid);
                    // add 'remove' operation to postponed operations
                    PostponedRemoveNode(sourceRoot, bCopySubtree, opid,
                                         endSourceIndex,
                                         endSourceIndex);

                    // recursively generate the diffgram subtree
                    GenerateDiffgramAdd(diffgramNode, sourceRoot.Left, targetRoot.Left);

                    // insert to diffgram tree
                    parent.InsertAtBeginning(diffgramNode);
                }
                else
                {
                    // output <node>
                    diffgramNode = new DiffgramPosition(sourceRoot);

                    // recursively generate the diffgram subtree
                    GenerateDiffgramMatch(diffgramNode, sourceRoot.Left, targetRoot.Left);

                    // insert to diffgram tree
                    if (diffgramNode._firstChildOp != null)
                        parent.InsertAtBeginning(diffgramNode);
                }
            }
        }

        private bool OnAdd(DiffgramParentOperation parent, int sourceBorderIndex, int targetBorderIndex)
        {
            EditScriptAdd addOp = this.editScript as EditScriptAdd;

            Debug.Assert(addOp._endTargetIndex == this.curTargetIndex);
            XmlDiffNode targetRoot = this.targetNodes[addOp._endTargetIndex];

            // add subtree or leaf node and no descendant node matches (= has been moved from somewhere else)
            if (addOp._startTargetIndex <= targetRoot.Left &&
                 !targetRoot.isSomeDescendantMatches)
            {
                switch (targetRoot.NodeType)
                {
                    case XmlDiffNodeType.ShrankNode:
                        XmlDiffShrankNode shrankNode = (XmlDiffShrankNode)targetRoot;

                        if (shrankNode.MoveOperationId == 0)
                            shrankNode.MoveOperationId = GenerateOperationID(XmlDiffDescriptorType.Move);

                        parent.InsertAtBeginning(new DiffgramCopy(shrankNode.MatchingShrankNode, true, shrankNode.MoveOperationId));
                        break;

                    case XmlDiffNodeType.XmlDeclaration:
                    case XmlDiffNodeType.DocumentType:
                    case XmlDiffNodeType.EntityReference:
                        parent.InsertAtBeginning(new DiffgramAddNode(targetRoot, 0));
                        break;

                    default:
                        if (!parent.MergeAddSubtreeAtBeginning(targetRoot))
                        {
                            parent.InsertAtBeginning(new DiffgramAddSubtrees(targetRoot, 0, !this.xmlDiff.IgnoreChildOrder));
                        }
                        break;
                }
                // adjust current position
                this.curTargetIndex = targetRoot.Left - 1;

                // adjust boundaries in the edit script or move to next edit script operation
                addOp._endTargetIndex = targetRoot.Left - 1;
                if (addOp._startTargetIndex > addOp._endTargetIndex)
                    this.editScript = this.editScript.nextEditScript;
            }
            // add single but non-leaf node, or some descendant matches (= has been moved from somewhere else )
            // -> recursively process diffgram subtree  
            else
            {
                Debug.Assert(!(targetRoot is XmlDiffShrankNode));
                DiffgramAddNode addNode = new DiffgramAddNode(targetRoot, 0);

                // adjust current position
                this.curTargetIndex--;

                // adjust boundaries in the edit script or move to next edit script operation
                addOp._endTargetIndex--;
                if (addOp._startTargetIndex > addOp._endTargetIndex)
                    this.editScript = this.editScript.nextEditScript;

                if (isBuildingAddTree)
                {
                    GenerateDiffgramAdd(addNode, sourceBorderIndex, targetRoot.Left);
                }
                else
                {
                    // switch to 'building add-tree' mode
                    postponedEditScript.Reset();
                    isBuildingAddTree = true;

                    // generate new tree
                    GenerateDiffgramAdd(addNode, sourceBorderIndex, targetRoot.Left);

                    isBuildingAddTree = false;

                    // attach postponed edit script to _editScript for further processing
                    if (postponedEditScript._firstES != null)
                    {
                        Debug.Assert(postponedEditScript._lastES != null);
                        Debug.Assert(postponedEditScript._startSourceIndex != 0);
                        Debug.Assert(postponedEditScript._endSourceIndex != 0);
                        this.curSourceIndex = postponedEditScript._endSourceIndex;
                        postponedEditScript._lastES.nextEditScript = this.editScript;
                        this.editScript = postponedEditScript._firstES;
                    }
                }

                // add attributes
                if (targetRoot.NodeType == XmlDiffNodeType.Element)
                    this.GenerateAddDiffgramForAttributes(addNode, (XmlDiffElement)targetRoot);

                parent.InsertAtBeginning(addNode);
            }

            // return true if positioning <node> element is needed in diffgram
            if (this.isChildOrderSignificant)
            {
                return !isBuildingAddTree;
            }
            else
            {
                return false;
            }
        }

        private void OnRemove(DiffgramParentOperation parent)
        {
            EditScriptRemove remOp = this.editScript as EditScriptRemove;

            Debug.Assert(remOp._endSourceIndex == this.curSourceIndex);
            XmlDiffNode sourceRoot = this.sourceNodes[remOp._endSourceIndex];

            // remove subtree or leaf node and no descendant node matches (=has been moved somewhere else)
            if (remOp._startSourceIndex <= sourceRoot.Left)
            {
                bool bShrankNode = sourceRoot is XmlDiffShrankNode;

                if (sourceRoot.isSomeDescendantMatches && !bShrankNode)
                {
                    DiffgramOperation newDiffOp = GenerateDiffgramRemoveWhenDescendantMatches((XmlDiffParentNode)sourceRoot);
                    if (isBuildingAddTree)
                    {
                        PostponedOperation(newDiffOp, sourceRoot.Left, remOp._endSourceIndex);
                    }
                    else
                    {
                        parent.InsertAtBeginning(newDiffOp);
                    }
                }
                else
                {
                    ulong opid = 0;
                    // shrank node -> output as 'move' operation
                    if (bShrankNode)
                    {
                        XmlDiffShrankNode shrankNode = (XmlDiffShrankNode)sourceRoot;
                        if (shrankNode.MoveOperationId == 0)
                            shrankNode.MoveOperationId = GenerateOperationID(XmlDiffDescriptorType.Move);
                        opid = shrankNode.MoveOperationId;

                        Debug.Assert(sourceRoot == this.sourceNodes[sourceRoot.Left]);
                    }

                    // insert 'remove' operation 
                    if (isBuildingAddTree)
                    {
                        PostponedRemoveSubtrees(sourceRoot, opid,
                            //AddToPosponedOperations( new DiffgramRemoveSubtrees( sourceRoot, opid ), 
                            sourceRoot.Left, remOp._endSourceIndex);
                    }
                    else
                    {
                        if (opid != 0 ||
                            !parent.MergeRemoveSubtreeAtBeginning(sourceRoot))
                        {
                            parent.InsertAtBeginning(new DiffgramRemoveSubtrees(sourceRoot, opid, !this.xmlDiff.IgnoreChildOrder));
                        }
                    }
                }

                // adjust current position
                this.curSourceIndex = sourceRoot.Left - 1;

                // adjust boundaries in the edit script or move to next edit script operation
                remOp._endSourceIndex = sourceRoot.Left - 1;
                if (remOp._startSourceIndex > remOp._endSourceIndex)
                    this.editScript = this.editScript.nextEditScript;
            }
            // remove single but non-leaf node or some descendant matches (=has been moved somewhere else)
            // -> recursively process diffgram subtree  
            else
            {
                Debug.Assert(!(sourceRoot is XmlDiffShrankNode));

                // adjust current position
                this.curSourceIndex--;

                // adjust boundaries in the edit script or move to next edit script operation
                remOp._endSourceIndex--;
                if (remOp._startSourceIndex > remOp._endSourceIndex)
                    this.editScript = this.editScript.nextEditScript;

                bool bRemoveSubtree = sourceRoot.NodeType != XmlDiffNodeType.Element;

                if (isBuildingAddTree)
                {
                    // add 'remove' to postponed operations 
                    PostponedRemoveNode(sourceRoot, bRemoveSubtree, 0,
                        //AddToPosponedOperations( new DiffgramRemoveNode( sourceRoot, bRemoveSubtree, 0 ), 
                        remOp._endSourceIndex + 1, remOp._endSourceIndex + 1);

                    // recursively parse subtree
                    GenerateDiffgramAdd(parent, sourceRoot.Left, this.targetNodes[this.curTargetIndex].Left);
                }
                else
                {
                    // 'remove' operation
                    DiffgramRemoveNode remNode = new DiffgramRemoveNode(sourceRoot, bRemoveSubtree, 0);

                    // parse subtree
                    GenerateDiffgramMatch(remNode, sourceRoot.Left, this.targetNodes[this.curTargetIndex].Left);

                    parent.InsertAtBeginning(remNode);
                }
            }
        }

        // produces <change> element in diffgram
        private void OnChange(DiffgramParentOperation parent)
        {
            EditScriptChange chOp = this.editScript as EditScriptChange;

            Debug.Assert(chOp._targetIndex == this.curTargetIndex);
            Debug.Assert(chOp._sourceIndex == this.curSourceIndex);

            XmlDiffNode sourceRoot = this.sourceNodes[chOp._sourceIndex];
            XmlDiffNode targetRoot = this.targetNodes[chOp._targetIndex];

            Debug.Assert(!(sourceRoot is XmlDiffShrankNode));
            Debug.Assert(!(targetRoot is XmlDiffShrankNode));

            // adjust current position
            this.curSourceIndex--;
            this.curTargetIndex--;

            // move to next edit script operation
            this.editScript = this.editScript.nextEditScript;

            DiffgramOperation diffgramNode = null;

            if (isBuildingAddTree)
            {
                // <add> changed node to the new location
                if (targetRoot.NodeType == XmlDiffNodeType.Element)
                    diffgramNode = new DiffgramAddNode(targetRoot, 0);
                else
                    diffgramNode = new DiffgramAddSubtrees(targetRoot, 0, !this.xmlDiff.IgnoreChildOrder);

                // <remove> old node from old location -> add to postponed operations
                bool bSubtree = sourceRoot.NodeType != XmlDiffNodeType.Element;
                PostponedRemoveNode(sourceRoot, bSubtree, 0,
                    //AddToPosponedOperations( new DiffgramRemoveNode( sourceRoot, bSubtree, 0 ), 
                    chOp._sourceIndex, chOp._sourceIndex);

                // recursively process children
                if (sourceRoot.Left < chOp._sourceIndex ||
                    targetRoot.Left < chOp._targetIndex)
                {
                    Debug.Assert(targetRoot.NodeType == XmlDiffNodeType.Element);
                    GenerateDiffgramAdd((DiffgramParentOperation)diffgramNode, sourceRoot.Left, targetRoot.Left);
                }

                // add attributes, if element
                if (targetRoot.NodeType == XmlDiffNodeType.Element)
                    GenerateAddDiffgramForAttributes((DiffgramParentOperation)diffgramNode, (XmlDiffElement)targetRoot);
            }
            else
            {
                ulong opid = 0;

                // change of namespace or prefix -> get the appropriate operation id
                if (!this.xmlDiff.IgnoreNamespaces &&
                     sourceRoot.NodeType == XmlDiffNodeType.Element)
                {
                    XmlDiffElement sourceEl = (XmlDiffElement)sourceRoot;
                    XmlDiffElement targetEl = (XmlDiffElement)targetRoot;

                    if (sourceEl.LocalName == targetEl.LocalName)
                    {
                        opid = GetNamespaceChangeOpid(sourceEl.NamespaceURI, sourceEl.Prefix,
                                                       targetEl.NamespaceURI, targetEl.Prefix);
                    }
                }

                if (sourceRoot.NodeType == XmlDiffNodeType.Element)
                {
                    if (XmlDiff.IsChangeOperationOnAttributesOnly(chOp._changeOp))
                        diffgramNode = new DiffgramPosition(sourceRoot);
                    else
                    {
                        Debug.Assert((int)chOp._changeOp == (int)XmlDiffOperation.ChangeElementName ||
                                      ((int)chOp._changeOp >= (int)XmlDiffOperation.ChangeElementNameAndAttr1 &&
                                        (int)chOp._changeOp <= (int)XmlDiffOperation.ChangeElementNameAndAttr2));

                        diffgramNode = new DiffgramChangeNode(sourceRoot, targetRoot, XmlDiffOperation.ChangeElementName, opid);
                    }

                    // recursively process children
                    if (sourceRoot.Left < chOp._sourceIndex ||
                        targetRoot.Left < chOp._targetIndex)
                    {
                        GenerateDiffgramMatch((DiffgramParentOperation)diffgramNode, sourceRoot.Left, targetRoot.Left);
                    }

                    GenerateChangeDiffgramForAttributes((DiffgramParentOperation)diffgramNode, (XmlDiffElement)sourceRoot, (XmlDiffElement)targetRoot);
                }
                else
                {
                    // '<change>'
                    diffgramNode = new DiffgramChangeNode(sourceRoot, targetRoot, chOp._changeOp, opid);
                    Debug.Assert(!sourceRoot.HasChildNodes);
                }
            }

            parent.InsertAtBeginning(diffgramNode);
        }

        private void OnEditScriptPostponed(DiffgramParentOperation parent, int targetBorderIndex)
        {
            EditScriptPostponed esp = (EditScriptPostponed)this.editScript;
            Debug.Assert(this.curSourceIndex == esp._endSourceIndex);

            DiffgramOperation diffOp = esp._diffOperation;
            int sourceStartIndex = esp._startSourceIndex;
            int sourceLeft = this.sourceNodes[esp._endSourceIndex].Left;

            // adjust current source index
            this.curSourceIndex = esp._startSourceIndex - 1;

            // move to next edit script
            this.editScript = esp.nextEditScript;

            // not a subtree or leaf node operation -> process child operations
            if (sourceStartIndex > sourceLeft)
            {
                GenerateDiffgramPostponed((DiffgramParentOperation)diffOp, ref this.editScript, sourceLeft, targetBorderIndex);
            }

            parent.InsertAtBeginning(diffOp);
        }

        // generates a new operation ID
        private ulong GenerateOperationID(XmlDiffDescriptorType descriptorType)
        {
            ulong opid = ++this.lastOperationID;

            if (descriptorType == XmlDiffDescriptorType.Move)
                this.moveDescriptors.Add(opid, opid);
            return opid;
        }

        private void PostponedRemoveNode(XmlDiffNode sourceNode, bool bSubtree, ulong operationID, int startSourceIndex, int endSourceIndex)
        {
            Debug.Assert(sourceNode != null);
            PostponedOperation(new DiffgramRemoveNode(sourceNode, bSubtree, operationID), startSourceIndex, endSourceIndex);
        }

        private void PostponedRemoveSubtrees(XmlDiffNode sourceNode, ulong operationID, int startSourceIndex, int endSourceIndex)
        {
            Debug.Assert(this.isBuildingAddTree);
            Debug.Assert(sourceNode != null);

            if (operationID == 0 && this.postponedEditScript._firstES != null)
            {
                Debug.Assert(this.postponedEditScript._lastES._startSourceIndex > endSourceIndex);

                DiffgramRemoveSubtrees remSubtrees = this.postponedEditScript._lastES._diffOperation as DiffgramRemoveSubtrees;
                if (remSubtrees != null &&
                    remSubtrees.SetNewFirstNode(sourceNode))
                {
                    this.postponedEditScript._lastES._startSourceIndex = startSourceIndex;
                    this.postponedEditScript._startSourceIndex = startSourceIndex;
                    return;
                }
            }

            PostponedOperation(new DiffgramRemoveSubtrees(sourceNode, operationID, !this.xmlDiff.IgnoreChildOrder), startSourceIndex, endSourceIndex);
        }

        private void PostponedOperation(DiffgramOperation op, int startSourceIndex, int endSourceIndex)
        {
            Debug.Assert(isBuildingAddTree);
            Debug.Assert(op != null);

            EditScriptPostponed es = new EditScriptPostponed(op, startSourceIndex, endSourceIndex);

            if (this.postponedEditScript._firstES == null)
            {
                this.postponedEditScript._firstES = es;
                this.postponedEditScript._lastES = es;
                this.postponedEditScript._startSourceIndex = startSourceIndex;
                this.postponedEditScript._endSourceIndex = endSourceIndex;
            }
            else
            {
                Debug.Assert(this.postponedEditScript._lastES != null);
                Debug.Assert(this.postponedEditScript._lastES._startSourceIndex > endSourceIndex);

                this.postponedEditScript._lastES.nextEditScript = es;
                this.postponedEditScript._lastES = es;

                this.postponedEditScript._startSourceIndex = startSourceIndex;
            }
        }

        private void GenerateChangeDiffgramForAttributes(DiffgramParentOperation diffgramParent, XmlDiffElement sourceElement, XmlDiffElement targetElement)
        {
            XmlDiffAttributeOrNamespace sourceAttr = sourceElement.attributes;
            XmlDiffAttributeOrNamespace targetAttr = targetElement.attributes;
            int nCompare;
            ulong opid;

            while (sourceAttr != null && targetAttr != null)
            {
                opid = 0;

                if (sourceAttr.NodeType == targetAttr.NodeType)
                {
                    if (sourceAttr.NodeType == XmlDiffNodeType.Attribute)
                    {
                        if ((nCompare = XmlDiffDocument.OrderStrings(sourceAttr.LocalName, targetAttr.LocalName)) == 0)
                        {
                            if (this.xmlDiff.IgnoreNamespaces)
                            {
                                if (XmlDiffDocument.OrderStrings(sourceAttr.Value, targetAttr.Value) == 0)
                                {
                                    // attributes match
                                    goto Next;
                                }
                            }
                            else
                            {
                                if (XmlDiffDocument.OrderStrings(sourceAttr.NamespaceURI, targetAttr.NamespaceURI) == 0 &&
                                     (this.xmlDiff.IgnorePrefixes || XmlDiffDocument.OrderStrings(sourceAttr.Prefix, targetAttr.Prefix) == 0) &&
                                    XmlDiffDocument.OrderStrings(sourceAttr.Value, targetAttr.Value) == 0)
                                {
                                    // attributes match
                                    goto Next;
                                }
                            }

                            diffgramParent.InsertAtBeginning(new DiffgramChangeNode(sourceAttr, targetAttr, XmlDiffOperation.ChangeAttr, 0));
                            goto Next;
                        }

                        goto AddRemove;
                    }
                    else // sourceAttr.NodeType != XmlDiffNodeType.Attribute 
                    {
                        if (this.xmlDiff.IgnorePrefixes)
                        {
                            if ((nCompare = XmlDiffDocument.OrderStrings(sourceAttr.NamespaceURI, targetAttr.NamespaceURI)) == 0)
                                goto Next;
                            else
                                goto AddRemove;
                        }
                        else if ((nCompare = XmlDiffDocument.OrderStrings(sourceAttr.Prefix, targetAttr.Prefix)) == 0)
                        {
                            if ((nCompare = XmlDiffDocument.OrderStrings(sourceAttr.NamespaceURI, targetAttr.NamespaceURI)) == 0)
                                goto Next;
                            else
                            {
                                // change of namespace
                                opid = GetNamespaceChangeOpid(sourceAttr.NamespaceURI, sourceAttr.Prefix,
                                                            targetAttr.NamespaceURI, targetAttr.Prefix);
                                goto AddRemoveBoth;
                            }
                        }
                        else
                        {
                            if ((nCompare = XmlDiffDocument.OrderStrings(sourceAttr.NamespaceURI, targetAttr.NamespaceURI)) == 0)
                            {
                                // change of prefix
                                opid = GetNamespaceChangeOpid(sourceAttr.NamespaceURI, sourceAttr.Prefix,
                                                            targetAttr.NamespaceURI, targetAttr.Prefix);
                                goto AddRemoveBoth;
                            }
                            else
                            {
                                goto AddRemove;
                            }
                        }
                    }
                }
                else // ( sourceAttr.NodeType != targetAttr.NodeType )
                {
                    if (sourceAttr.NodeType == XmlDiffNodeType.Namespace)
                        goto RemoveSource;
                    else
                        goto AddTarget;
                }

            Next:
                sourceAttr = (XmlDiffAttributeOrNamespace)sourceAttr.nextSibling;
                targetAttr = (XmlDiffAttributeOrNamespace)targetAttr.nextSibling;
                continue;

            AddRemove:
                if (nCompare == -1)
                    goto RemoveSource;
                else
                {
                    Debug.Assert(nCompare == 1);
                    goto AddTarget;
                }

            AddRemoveBoth:
                if (!diffgramParent.MergeRemoveAttributeAtBeginning(sourceAttr))
                    diffgramParent.InsertAtBeginning(new DiffgramRemoveNode(sourceAttr, true, opid));
                sourceAttr = (XmlDiffAttributeOrNamespace)sourceAttr.nextSibling;

                diffgramParent.InsertAtBeginning(new DiffgramAddNode(targetAttr, opid));
                targetAttr = (XmlDiffAttributeOrNamespace)targetAttr.nextSibling;
                continue;

            RemoveSource:
                if (!diffgramParent.MergeRemoveAttributeAtBeginning(sourceAttr))
                    diffgramParent.InsertAtBeginning(new DiffgramRemoveNode(sourceAttr, true, opid));
                sourceAttr = (XmlDiffAttributeOrNamespace)sourceAttr.nextSibling;
                continue;

            AddTarget:
                diffgramParent.InsertAtBeginning(new DiffgramAddNode(targetAttr, opid));
                targetAttr = (XmlDiffAttributeOrNamespace)targetAttr.nextSibling;
                continue;
            }

            while (sourceAttr != null)
            {
                if (!diffgramParent.MergeRemoveAttributeAtBeginning(sourceAttr))
                    diffgramParent.InsertAtBeginning(new DiffgramRemoveNode(sourceAttr, true, 0));
                sourceAttr = (XmlDiffAttributeOrNamespace)sourceAttr.nextSibling;
            }

            while (targetAttr != null)
            {
                diffgramParent.InsertAtBeginning(new DiffgramAddNode(targetAttr, 0));
                targetAttr = (XmlDiffAttributeOrNamespace)targetAttr.nextSibling;
            }
        }

        private void GenerateAddDiffgramForAttributes(DiffgramParentOperation diffgramParent, XmlDiffElement targetElement)
        {
            XmlDiffAttributeOrNamespace attr = targetElement.attributes;
            while (attr != null)
            {
                diffgramParent.InsertAtBeginning(new DiffgramAddNode(attr, 0));
                attr = (XmlDiffAttributeOrNamespace)attr.nextSibling;
            }
        }

        private void GenerateDiffgramAdd(DiffgramParentOperation parent, int sourceBorderIndex, int targetBorderIndex)
        {
            while (this.curTargetIndex >= targetBorderIndex)
            {
                Debug.Assert(this.editScript != null);

                switch (this.editScript.Operation)
                {
                    case EditScriptOperation.Match:
                        OnMatch(parent, false);
                        break;
                    case EditScriptOperation.Add:
                        OnAdd(parent, sourceBorderIndex, targetBorderIndex);
                        break;
                    case EditScriptOperation.Remove:
                        OnRemove(parent);
                        break;
                    case EditScriptOperation.ChangeNode:
                        OnChange(parent);
                        break;
                    case EditScriptOperation.EditScriptPostponed:
                        OnEditScriptPostponed(parent, targetBorderIndex);
                        break;
                    default:
                        Debug.Assert(false, "Invalid edit script operation type in final edit script.");
                        break;
                }
            }
        }

        private DiffgramOperation GenerateDiffgramRemoveWhenDescendantMatches(XmlDiffNode sourceParent)
        {
            Debug.Assert(sourceParent.isSomeDescendantMatches);
            Debug.Assert(sourceParent.NodeType != XmlDiffNodeType.ShrankNode);

            DiffgramParentOperation diffOp = new DiffgramRemoveNode(sourceParent, false, 0);
            XmlDiffNode child = ((XmlDiffParentNode)sourceParent).firstChildNode;
            while (child != null)
            {
                if (child.NodeType == XmlDiffNodeType.ShrankNode)
                {
                    XmlDiffShrankNode shrankNode = (XmlDiffShrankNode)child;

                    if (shrankNode.MoveOperationId == 0)
                        shrankNode.MoveOperationId = GenerateOperationID(XmlDiffDescriptorType.Move);

                    diffOp.InsertAtEnd(new DiffgramRemoveSubtrees(child, shrankNode.MoveOperationId, !this.xmlDiff.IgnoreChildOrder));

                }
                else if (child.HasChildNodes && child.isSomeDescendantMatches)
                {
                    diffOp.InsertAtEnd(GenerateDiffgramRemoveWhenDescendantMatches((XmlDiffParentNode)child));
                }
                else
                {
                    if (!diffOp.MergeRemoveSubtreeAtEnd(child))
                        diffOp.InsertAtEnd(new DiffgramRemoveSubtrees(child, 0, !this.xmlDiff.IgnoreChildOrder));
                }

                child = child.nextSibling;
            }
            return diffOp;
        }

        private ulong GetNamespaceChangeOpid(string oldNamespaceURI, string oldPrefix, string newNamespaceURI, string newPrefix)
        {
            Debug.Assert(!this.xmlDiff.IgnoreNamespaces);

            ulong opid = 0;

            // namespace change
            if (oldNamespaceURI != newNamespaceURI)
            {
                // prefix must remain the same
                if (oldPrefix != newPrefix)
                    return 0;

                // lookup this change in the list of namespace changes
                NamespaceChange nsChange = this.namespaceChangeDescr;
                while (nsChange != null)
                {
                    if (nsChange._oldNS == oldNamespaceURI &&
                         nsChange._prefix == oldPrefix &&
                         nsChange._newNS == newNamespaceURI)
                    {
                        return nsChange._opid;
                    }

                    nsChange = nsChange._next;
                }

                // the change record was not found -> create a new one
                opid = GenerateOperationID(XmlDiffDescriptorType.NamespaceChange);
                this.namespaceChangeDescr = new NamespaceChange(oldPrefix, oldNamespaceURI, newNamespaceURI, opid, this.namespaceChangeDescr);
            }
            // prefix change
            else if (!this.xmlDiff.IgnorePrefixes &&
                      oldPrefix != newPrefix)
            {
                // lookup this change in the list of prefix changes
                PrefixChange prefixChange = this.prefixChangeDescr;
                while (prefixChange != null)
                {
                    if (prefixChange._NS == oldNamespaceURI &&
                         prefixChange._oldPrefix == oldPrefix &&
                         prefixChange._newPrefix == newPrefix)
                    {
                        return prefixChange._opid;
                    }

                    prefixChange = prefixChange._next;
                }

                // the change record was not found -> create a new one
                opid = GenerateOperationID(XmlDiffDescriptorType.PrefixChange);
                this.prefixChangeDescr = new PrefixChange(oldPrefix, newPrefix, oldNamespaceURI, opid, this.prefixChangeDescr);
            }

            return opid;
        }

        private void GenerateDiffgramPostponed(DiffgramParentOperation parent, ref EditScript editScript, int sourceBorderIndex, int targetBorderIndex)
        {
            while (this.curSourceIndex >= sourceBorderIndex && editScript != null)
            {
                EditScriptPostponed esp = editScript as EditScriptPostponed;
                if (esp == null)
                {
                    GenerateDiffgramMatch(parent, sourceBorderIndex, targetBorderIndex);
                    return;
                }

                Debug.Assert(esp._endSourceIndex == this.curSourceIndex);

                int sourceStartIndex = esp._startSourceIndex;
                int sourceLeft = this.sourceNodes[esp._endSourceIndex].Left;
                DiffgramOperation diffOp = esp._diffOperation;

                // adjust current source index
                this.curSourceIndex = esp._startSourceIndex - 1;

                // move to next edit script
                editScript = esp.nextEditScript;

                // not a subtree or leaf node operation -> process child operations
                if (sourceStartIndex > sourceLeft)
                {
                    GenerateDiffgramPostponed((DiffgramParentOperation)diffOp, ref editScript, sourceLeft, targetBorderIndex);
                }

                // insert operation
                parent.InsertAtBeginning(diffOp);
            }
        }
        #endregion

        internal struct PostponedEditScriptInfo
        {
            internal EditScriptPostponed _firstES;
            internal EditScriptPostponed _lastES;
            internal int _startSourceIndex;
            internal int _endSourceIndex;

            internal void Reset()
            {
                _firstES = null;
                _lastES = null;
                _startSourceIndex = 0;
                _endSourceIndex = 0;
            }
        }

        internal class NamespaceChange
        {
            internal string _prefix;
            internal string _oldNS;
            internal string _newNS;
            internal ulong _opid;
            internal NamespaceChange _next;

            internal NamespaceChange(string prefix, string oldNamespace, string newNamespace,
                                    ulong opid, NamespaceChange next)
            {
                _prefix = prefix;
                _oldNS = oldNamespace;
                _newNS = newNamespace;
                _opid = opid;
                _next = next;
            }
        }

        internal class PrefixChange
        {
            internal string _oldPrefix;
            internal string _newPrefix;
            internal string _NS;
            internal ulong _opid;
            internal PrefixChange _next;

            internal PrefixChange(string oldPrefix, string newPrefix, string ns, ulong opid,
                                 PrefixChange next)
            {
                _oldPrefix = oldPrefix;
                _newPrefix = newPrefix;
                _NS = ns;
                _opid = opid;
                _next = next;
            }
        }
    }

    
}
