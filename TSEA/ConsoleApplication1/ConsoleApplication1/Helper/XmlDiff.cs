

namespace Sam.XmlDiffPath
{
	using System;
    using System.Collections;
	using System.Collections.Generic;
    using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml;

	class XmlDiff
    {
        #region Fields
        internal XmlDiffDocument sourceDiffDoc = null;
        internal XmlDiffDocument targetDiffDoc = null;
        internal TriStateBool fragments = TriStateBool.DontKnown;

        // nodes sorted according to post-order numbering
        internal XmlDiffNode[] sourceNodes = null;
        internal XmlDiffNode[] targetNodes = null;

        private XmlDiffAlgorithm algorithm = XmlDiffAlgorithm.Auto;

        // Options flags
        private bool isIgnoreChildOrder = false;
        private bool isIgnoreComments = false;
        private bool isIgnorePI = false;
        private bool isIgnoreWhitespace = false;
        private bool isIgnoreNamespaces = false;
        private bool isIgnorePrefixes = false;
        private bool isIgnoreXmlDecl = false;
        private bool isIgnoreDtd = false;

        private const int MininumNodesForQuicksort = 5;
        private const int MaxTotalNodesCountForTreeDistance = 256;

        public const string NamespaceUri = "http://schemas.microsoft.com/xmltools/2002/xmldiff";
        internal const string Prefix = "xd";
        internal const string XmlnsNamespaceUri = "http://www.w3.org/2000/xmlns/";
        #endregion

        #region Properties
        /// <summary>
        /// Options used when comparing XML documents/fragments.
        /// </summary>
        public XmlDiffOptions Options
        {
            set
            {
                IgnoreChildOrder = (((int)value & (int)(XmlDiffOptions.IgnoreChildOrder)) > 0);
                IgnoreComments = (((int)value & (int)(XmlDiffOptions.IgnoreComments)) > 0);
                IgnorePI = (((int)value & (int)(XmlDiffOptions.IgnorePI)) > 0);
                IgnoreWhitespace = (((int)value & (int)(XmlDiffOptions.IgnoreWhitespace)) > 0);
                IgnoreNamespaces = (((int)value & (int)(XmlDiffOptions.IgnoreNamespaces)) > 0);
                IgnorePrefixes = (((int)value & (int)(XmlDiffOptions.IgnorePrefixes)) > 0);
                IgnoreXmlDecl = (((int)value & (int)(XmlDiffOptions.IgnoreXmlDecl)) > 0);
                IgnoreDtd = (((int)value & (int)(XmlDiffOptions.IgnoreDtd)) > 0);
            }
        }

        public XmlDiffAlgorithm Algorithm { get { return this.algorithm; } set { this.algorithm = value; } }

        /// <summary>
        ///    If true, the order of child nodes of each element will be ignored when comparing 
        ///    the documents/fragments.
        /// </summary>
        public bool IgnoreChildOrder { get { return isIgnoreChildOrder; } set { isIgnoreChildOrder = value; } }

        /// <summary>
        ///    If true, all comments in the compared documents/fragments will be ignored.
        /// </summary>
        public bool IgnoreComments { get { return isIgnoreComments; } set { isIgnoreComments = value; } }

        /// <summary>
        ///    If true, all processing instructions in the compared documents/fragments will be ignored.
        /// </summary>
        public bool IgnorePI { get { return isIgnorePI; } set { isIgnorePI = value; } }

        /// <summary>
        ///    If true, all whitespace nodes in the compared documents/fragments will be ignored. Also, all
        ///    text nodes and values of attributes will be normalized; whitespace sequences will be replaced
        ///    by single space and beginning and trailing whitespaces will be trimmed.
        /// </summary>
        public bool IgnoreWhitespace { get { return isIgnoreWhitespace; } set { isIgnoreWhitespace = value; } }

        /// <summary>
        ///    If true, the namespaces will be ignored when comparing the names of elements and attributes.
        ///    This also mean that the prefixes will be ignored too as if the IgnorePrefixes option is true.
        /// </summary>
        public bool IgnoreNamespaces { get { return isIgnoreNamespaces; } set { isIgnoreNamespaces = value; } }

        /// <summary>
        ///    If true, the prefixes will be ignored when comparing the names of elements and attributes. 
        ///    The namespaces will not be ignored unless IgnoreNamespaces flag is true.
        /// </summary>
        public bool IgnorePrefixes { get { return isIgnorePrefixes; } set { isIgnorePrefixes = value; } }

        /// <summary>
        ///    If true, the xml declarations will not be compared.
        /// </summary>
        public bool IgnoreXmlDecl { get { return isIgnoreXmlDecl; } set { isIgnoreXmlDecl = value; } }

        /// <summary>
        ///    If true, the xml declarations will not be compared.
        /// </summary>
        public bool IgnoreDtd { get { return isIgnoreDtd; } set { isIgnoreDtd = value; } }
        #endregion

        #region Constructors
        public XmlDiff(XmlDiffOptions options)
        {
            this.Options = options;
        }
        #endregion

        #region Public Methods
        /// <summary>
		///    Compares two XML documents or fragments. 
		///    If the diffgramWriter parameter is not null it will contain the list of changes 
		///    between the two XML documents/fragments (diffgram).
		/// </summary>
		/// <param name="sourceFile">The original xml document or fragment filename</param>
		/// <param name="changedFile">The changed xml document or fragment filename.</param>
		/// <param name="bFragments">If true, the passed files contain xml fragments; otherwise the files must contain xml documents.</param>
		/// <param name="diffgramWriter">XmlWriter object for returning the list of changes (diffgram).</param>
		/// <returns>True, if the documents/fragments are identical.</returns>
		public bool Compare(string sourceFile, string changedFile, bool bFragments, XmlWriter diffgramWriter)
		{
			if (sourceFile == null)
				throw new ArgumentNullException("sourceFile");
			if (changedFile == null)
				throw new ArgumentNullException("changedFile");

			XmlReader sourceReader = null;
			XmlReader targetReader = null;

			try
			{
				//_fragments = bFragments ? TriStateBool.Yes : TriStateBool.No;

				if (bFragments)
					OpenFragments(sourceFile, changedFile, ref sourceReader, ref targetReader);
				else
					OpenDocuments(sourceFile, changedFile, ref sourceReader, ref targetReader);

				return Compare(sourceReader, targetReader, diffgramWriter);
			}
			finally
			{
				if (sourceReader != null)
				{
					sourceReader.Close();
					sourceReader = null;
				}
				if (targetReader != null)
				{
					targetReader.Close();
					targetReader = null;
				}
			}
		}

        /// <summary>
        ///    Compares two XML documents or fragments.
        ///    If the diffgramWriter parameter is not null it will contain the list of changes 
        ///    between the two XML documents/fragments (diffgram).
        /// </summary>
        /// <param name="sourceReader">XmlReader representing the original xml document or fragment.</param>
        /// <param name="changedFile">XmlReaser representing the changed xml document or fragment.</param>
        /// <param name="diffgramWriter">XmlWriter object for returning the list of changes (diffgram).</param>
        /// <returns>True, if the documents/fragments are identical.</returns>
        public bool Compare(XmlReader sourceReader, XmlReader changedReader, XmlWriter diffgramWriter)
        {
            if (sourceReader == null)
                throw new ArgumentNullException("sourceReader");
            if (changedReader == null)
                throw new ArgumentNullException("changedReader");

            try
            {
                XmlHash xmlHash = new XmlHash(this);

                // load source document
                sourceDiffDoc = new XmlDiffDocument(this);
                sourceDiffDoc.Load(sourceReader, xmlHash);

                // load target document
                targetDiffDoc = new XmlDiffDocument(this);
                targetDiffDoc.Load(changedReader, xmlHash);

                if (fragments == TriStateBool.DontKnown)
                {
                    fragments = (sourceDiffDoc.IsFragment || targetDiffDoc.IsFragment) ? TriStateBool.Yes : TriStateBool.No;
                }

                // compare
                return Diff(diffgramWriter);
            }
            finally
            {
                sourceDiffDoc = null;
                targetDiffDoc = null;
            }
        }
		#endregion

        #region Internal Methods
        internal string GetXmlDiffOptionsString()
        {
            string options = string.Empty;
            if (isIgnoreChildOrder) options += XmlDiffOptions.IgnoreChildOrder.ToString() + " ";
            if (isIgnoreComments) options += XmlDiffOptions.IgnoreComments.ToString() + " ";
            if (isIgnoreNamespaces) options += XmlDiffOptions.IgnoreNamespaces.ToString() + " ";
            if (isIgnorePI) options += XmlDiffOptions.IgnorePI.ToString() + " ";
            if (isIgnorePrefixes) options += XmlDiffOptions.IgnorePrefixes.ToString() + " ";
            if (isIgnoreWhitespace) options += XmlDiffOptions.IgnoreWhitespace.ToString() + " ";
            if (isIgnoreXmlDecl) options += XmlDiffOptions.IgnoreXmlDecl.ToString() + " ";
            if (isIgnoreDtd) options += XmlDiffOptions.IgnoreDtd.ToString() + " ";
            if (options == string.Empty) options = XmlDiffOptions.None.ToString();
            options.Trim();

            return options;
        }

        internal static void SortNodesByPosition(ref XmlDiffNode firstNode, ref XmlDiffNode lastNode, ref XmlDiffNode firstPreviousSibbling)
        {
            XmlDiffParentNode parent = firstNode.parent;

            // find previous sibling node for the first node
            if (firstPreviousSibbling == null &&
                 firstNode != parent.firstChildNode)
            {
                firstPreviousSibbling = parent.firstChildNode;
                while (firstPreviousSibbling.nextSibling != firstNode)
                    firstPreviousSibbling = firstPreviousSibbling.nextSibling;
            }

            // save the next sibling node for the last node
            XmlDiffNode lastNextSibling = lastNode.nextSibling;
            lastNode.nextSibling = null;

            // count the number of nodes to sort
            int count = 0;
            XmlDiffNode curNode = firstNode;
            while (curNode != null)
            {
                count++;
                curNode = curNode.nextSibling;
            }

            Debug.Assert(count > 0);
            if (count >= MininumNodesForQuicksort)
                QuickSortNodes(ref firstNode, ref lastNode, count, firstPreviousSibbling, lastNextSibling);
            else
                SlowSortNodes(ref firstNode, ref lastNode, firstPreviousSibbling, lastNextSibling);
        }

        internal static string NormalizeText(string text)
        {
            char[] chars = text.ToCharArray();
            int i = 0;
            int j = 0;

            for (; ; )
            {
                while (j < chars.Length && IsWhitespace(text[j]))
                    j++;

                while (j < chars.Length && !IsWhitespace(text[j]))
                    chars[i++] = chars[j++];

                if (j < chars.Length)
                {
                    chars[i++] = ' ';
                    j++;
                }
                else
                {
                    if (j == 0)
                        return string.Empty;

                    if (IsWhitespace(chars[j - 1]))
                        i--;

                    return new string(chars, 0, i);
                }
            }
        }

        internal static bool IsWhitespace(char c)
        {
            return (c == ' ' ||
                     c == '\t' ||
                     c == '\n' ||
                     c == '\r');
        }

        internal static string NormalizeXmlDeclaration(string value)
        {
            value = value.Replace('\'', '"');
            return NormalizeText(value);
        }

        internal static bool IsChangeOperation(XmlDiffOperation op)
        {
            return ((int)op >= (int)XmlDiffOperation.ChangeElementName) &&
                   ((int)op <= (int)XmlDiffOperation.ChangeDTD);
        }

        internal static bool IsChangeOperationOnAttributesOnly(XmlDiffOperation op)
        {
            return (int)op >= (int)XmlDiffOperation.ChangeElementAttr1 && (int)op <= (int)XmlDiffOperation.ChangeElementAttr3;
        }
        #endregion

        #region Private Methods
        private void OpenFragments(String sourceFile, String changedFile, ref XmlReader sourceReader, ref XmlReader changedReader)
		{
			FileStream sourceStream = null;
			FileStream changedStream = null;

			try
			{
				XmlNameTable nameTable = new NameTable();
				XmlParserContext sourceParserContext = new XmlParserContext(nameTable,
																			new XmlNamespaceManager(nameTable),
																			string.Empty,
																			System.Xml.XmlSpace.Default);
				XmlParserContext changedParserContext = new XmlParserContext(nameTable,
																			new XmlNamespaceManager(nameTable),
																			string.Empty,
																			System.Xml.XmlSpace.Default);
				sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
				changedStream = new FileStream(changedFile, FileMode.Open, FileAccess.Read);

				XmlTextReader tr = new XmlTextReader(sourceStream, XmlNodeType.Element, sourceParserContext);
				tr.XmlResolver = null;
				sourceReader = tr;

				tr = new XmlTextReader(changedStream, XmlNodeType.Element, changedParserContext);
				tr.XmlResolver = null;
				changedReader = tr;
			}
			catch
			{
				if (sourceStream != null)
					sourceStream.Close();
				if (changedStream != null)
					changedStream.Close();
				throw;
			}
		}

		private void OpenDocuments(String sourceFile, String changedFile, ref XmlReader sourceReader, ref XmlReader changedReader)
		{
			XmlTextReader tr = new XmlTextReader(sourceFile);
			tr.XmlResolver = null;
			sourceReader = tr;

			tr = new XmlTextReader(changedFile);
			tr.XmlResolver = null;
			changedReader = tr;
		}

        /// <summary>
        /// Make differencing between two XML files.
        /// </summary>
        /// <param name="diffgramWriter"></param>
        /// <returns></returns>
        private bool Diff(XmlWriter diffgramWriter)
        {
            if (diffgramWriter == null)
            {
                return false;
            }

            try
            {
                // compare hash values of root nodes and return if same (the hash values were computed during load)
                if (this.IdenticalSubtrees(sourceDiffDoc, targetDiffDoc))
                {
                    if (diffgramWriter != null)
                    {
                        Diffgram emptyDiffgram = new DiffgramGenerator(this).GenerateEmptyDiffgram();

                        emptyDiffgram.WriteTo(diffgramWriter);
                        diffgramWriter.Flush();
                    }

                    return true;
                }

                // Match & shrink identical subtrees
                this.MatchIdenticalSubtrees();

                Diffgram diffgram = null;

                // Choose differencing algorithm
                switch (algorithm)
                {
                    case XmlDiffAlgorithm.Fast:
                        diffgram = this.WalkTreeAlgorithm();
                        break;
                    case XmlDiffAlgorithm.Precise:
                        diffgram = this.ZhangShashaAlgorithm();
                        break;
                    case XmlDiffAlgorithm.Auto:
                        if (sourceDiffDoc.NodesCount + targetDiffDoc.NodesCount <= MaxTotalNodesCountForTreeDistance)
                            diffgram = this.ZhangShashaAlgorithm();
                        else
                            diffgram = this.WalkTreeAlgorithm();
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                // Output the diffgram
                if (diffgramWriter != null)
                {
                    diffgram.WriteTo(diffgramWriter);
                    diffgramWriter.Flush();
                }
            }
            finally
            {
                sourceDiffDoc = null;
                targetDiffDoc = null;
            }

            return false;
        }

        /// <summary>
        /// Check if two subtrees are identical.
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        private bool IdenticalSubtrees(XmlDiffNode node1, XmlDiffNode node2)
        {
            if (node1.HashValue != node2.HashValue)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Finds identical subtrees in both trees and shrinks them into XmlDiffShrankNode instances
        /// </summary>
        private void MatchIdenticalSubtrees()
        {
            Hashtable sourceUnmatchedNodes = new Hashtable(16);
            Hashtable targetUnmatchedNodes = new Hashtable(16);

            Queue sourceNodesToExpand = new Queue(16);
            Queue targetNodesToExpand = new Queue(16);

            sourceNodesToExpand.Enqueue(sourceDiffDoc);
            targetNodesToExpand.Enqueue(targetDiffDoc);

            while (sourceNodesToExpand.Count > 0 || targetNodesToExpand.Count > 0)
            {
                #region Expanding the nodes in the source file.
                // Expand next level of source nodes and add them to the sourceUnmatchedNodes hashtable.
                // Leave the parents of expanded nodes in the sourceNodesToExpand queue for later use.
                IEnumerator en = sourceNodesToExpand.GetEnumerator();
                while (en.MoveNext())
                {
                    XmlDiffParentNode sourceParentNode = (XmlDiffParentNode)en.Current;
                    Debug.Assert(!sourceParentNode.isExpanded);

                    sourceParentNode.isExpanded = true;

                    if (!sourceParentNode.HasChildNodes)
                        continue;

                    // handle the child nodes with Deep-First Traversal Algorithm
                    XmlDiffNode curSourceNode = sourceParentNode.firstChildNode;
                    while (curSourceNode != null)
                    {
                        AddNodeToHashTable(sourceUnmatchedNodes, curSourceNode);
                        curSourceNode = curSourceNode.nextSibling;
                    }
                }
                #endregion

                #region Expanding the nodes in the changed file; and meanwhile, match the nodes against those (stored in the sourceUnmatchedNodes hashtable) in the source file.
                // Expand next level of target nodes and try to match them against the sourceUnmatchedNodes hashtable.
                // to find matching node. 
                int count = targetNodesToExpand.Count;
                for (int i = 0; i < count; i++)
                {
                    XmlDiffParentNode targetParentNode = (XmlDiffParentNode)targetNodesToExpand.Dequeue();
                    Debug.Assert(!targetParentNode.isExpanded);

                    targetParentNode.isExpanded = true;

                    if (!targetParentNode.HasChildNodes)
                        continue;

                    XmlDiffNode curTargetNode = targetParentNode.firstChildNode;
                    while (curTargetNode != null)
                    {
                        Debug.Assert(!(curTargetNode is XmlDiffAttributeOrNamespace));

                        // try to match
                        XmlDiffNode firstSourceNode = null;
                        XmlDiffNodeListHead matchingSourceNodes = (XmlDiffNodeListHead)sourceUnmatchedNodes[curTargetNode.HashValue];

                        if (matchingSourceNodes != null)
                        {
                            // find matching node and remove it from the hashtable
                            firstSourceNode = LocateAndRemoveMatchingNode(sourceUnmatchedNodes, matchingSourceNodes, curTargetNode);
                        }

                        // no match
                        if (firstSourceNode == null ||
                            // do not shrink XML declarations and DTD
                             (int)curTargetNode.NodeType < 0)
                        {
                            if (curTargetNode.HasChildNodes)
                                targetNodesToExpand.Enqueue(curTargetNode);
                            else
                                curTargetNode.isExpanded = true;

                            AddNodeToHashTable(targetUnmatchedNodes, curTargetNode);
                            curTargetNode = curTargetNode.nextSibling;
                            continue;
                        }

                        RemoveAncestors(sourceUnmatchedNodes, firstSourceNode);
                        RemoveDescendants(sourceUnmatchedNodes, firstSourceNode);

                        RemoveAncestors(targetUnmatchedNodes, curTargetNode);
                        // there are no target node descendants in the hash table

                        // find matching interval - starts at startSourceNode and startTargetNode
                        XmlDiffNode firstTargetNode = curTargetNode;
                        XmlDiffNode lastSourceNode = firstSourceNode;
                        XmlDiffNode lastTargetNode = firstTargetNode;

                        curTargetNode = curTargetNode.nextSibling;
                        XmlDiffNode curSourceNode = firstSourceNode.nextSibling;

                        while (curTargetNode != null &&
                                curSourceNode != null &&
                                curSourceNode.NodeType != XmlDiffNodeType.ShrankNode)
                        {
                            // still matches and the nodes has not been matched elsewhere
                            if (IdenticalSubtrees(curSourceNode, curTargetNode) &&
                                 sourceUnmatchedNodes.Contains(curSourceNode.HashValue))
                            {
                                RemoveNode(sourceUnmatchedNodes, curSourceNode);
                                RemoveDescendants(sourceUnmatchedNodes, curSourceNode);
                            }
                            // no match -> end of interval
                            else
                                break;

                            lastSourceNode = curSourceNode;
                            curSourceNode = curSourceNode.nextSibling;

                            lastTargetNode = curTargetNode;
                            curTargetNode = curTargetNode.nextSibling;
                        }

                        if (firstSourceNode != lastSourceNode ||
                             firstSourceNode.NodeType != XmlDiffNodeType.Element)
                        {
                            ShrinkNodeInterval(firstSourceNode, lastSourceNode, firstTargetNode, lastTargetNode);
                        }
                        else
                        {
                            XmlDiffElement e = (XmlDiffElement)firstSourceNode;
                            if (e.FirstChildNode != null || e.attributes != null)
                            {
                                ShrinkNodeInterval(firstSourceNode, lastSourceNode, firstTargetNode, lastTargetNode);
                            }
                        }
                    }
                }
                #endregion

                #region Walk through the newly expanded source nodes (=children of nodes in sourceNodesToExpand queue) and try to match them against targetUnmatchedNodes hashtable.
                count = sourceNodesToExpand.Count;
                for (int i = 0; i < count; i++)
                {
                    XmlDiffParentNode sourceParentNode = (XmlDiffParentNode)sourceNodesToExpand.Dequeue();
                    Debug.Assert(sourceParentNode.isExpanded);

                    if (!sourceParentNode.HasChildNodes)
                        continue;

                    XmlDiffNode curSourceNode = sourceParentNode.firstChildNode;
                    while (curSourceNode != null)
                    {
                        // it it's an attribute or the node has already been matched -> continue
                        Debug.Assert(!(curSourceNode is XmlDiffAttributeOrNamespace));
                        if (curSourceNode is XmlDiffShrankNode || !this.NodeInHashTable(sourceUnmatchedNodes, curSourceNode))
                        {
                            curSourceNode = curSourceNode.nextSibling;
                            continue;
                        }

                        // try to match
                        XmlDiffNode firstTargetNode = null;
                        XmlDiffNodeListHead matchingTargetNodes = (XmlDiffNodeListHead)targetUnmatchedNodes[curSourceNode.HashValue];

                        if (matchingTargetNodes != null)
                        {
                            // find matching node and remove it from the hashtable
                            firstTargetNode = this.LocateAndRemoveMatchingNode(targetUnmatchedNodes, matchingTargetNodes, curSourceNode);
                        }

                        // no match
                        if (firstTargetNode == null ||
                            // do not shrink XML declarations and DTD
                             (int)curSourceNode.NodeType < 0)
                        {
                            if (curSourceNode.HasChildNodes)
                                sourceNodesToExpand.Enqueue(curSourceNode);
                            else
                                curSourceNode.isExpanded = true;

                            curSourceNode = curSourceNode.nextSibling;
                            continue;
                        }

                        this.RemoveAncestors(targetUnmatchedNodes, firstTargetNode);
                        this.RemoveDescendants(targetUnmatchedNodes, firstTargetNode);

                        if (!this.RemoveNode(sourceUnmatchedNodes, curSourceNode))
                            Debug.Assert(false);
                        this.RemoveAncestors(sourceUnmatchedNodes, curSourceNode);
                        // there are no source node descendants in the hash table

                        Debug.Assert(!(curSourceNode is XmlDiffAttributeOrNamespace));

                        // find matching interval - starts at startSourceNode and startTargetNode
                        XmlDiffNode firstSourceNode = curSourceNode;
                        XmlDiffNode lastSourceNode = firstSourceNode;
                        XmlDiffNode lastTargetNode = firstTargetNode;

                        curSourceNode = curSourceNode.nextSibling;
                        XmlDiffNode curTargetNode = firstTargetNode.nextSibling;

                        while (curSourceNode != null && curTargetNode != null && curTargetNode.NodeType != XmlDiffNodeType.ShrankNode)
                        {
                            // still matches and the nodes has not been matched elsewhere
                            if (IdenticalSubtrees(curSourceNode, curTargetNode) &&
                                sourceUnmatchedNodes.Contains(curSourceNode.HashValue) &&
                                targetUnmatchedNodes.Contains(curTargetNode.HashValue))
                            {
                                RemoveNode(sourceUnmatchedNodes, curSourceNode);
                                RemoveDescendants(sourceUnmatchedNodes, curSourceNode);

                                RemoveNode(targetUnmatchedNodes, curTargetNode);
                                RemoveDescendants(targetUnmatchedNodes, curTargetNode);
                            }
                            // no match -> end of interval
                            else
                            {
                                break;
                            }

                            lastSourceNode = curSourceNode;
                            curSourceNode = curSourceNode.nextSibling;

                            lastTargetNode = curTargetNode;
                            curTargetNode = curTargetNode.nextSibling;
                        }

                        if (firstSourceNode != lastSourceNode ||
                             firstSourceNode.NodeType != XmlDiffNodeType.Element)
                        {
                            ShrinkNodeInterval(firstSourceNode, lastSourceNode, firstTargetNode, lastTargetNode);
                        }
                        else
                        {
                            XmlDiffElement e = (XmlDiffElement)firstSourceNode;
                            if (e.FirstChildNode != null || e.attributes != null)
                            {
                                ShrinkNodeInterval(firstSourceNode, lastSourceNode, firstTargetNode, lastTargetNode);
                            }
                        }
                    }
                }
                #endregion

            }
        }

        private void AddNodeToHashTable(Hashtable hashtable, XmlDiffNode node)
        {
            Debug.Assert(hashtable != null);
            Debug.Assert(node != null);
            Debug.Assert(node.NodeType != XmlDiffNodeType.ShrankNode);

            ulong hashValue = node.HashValue;

            XmlDiffNodeListHead nodeListHead = (XmlDiffNodeListHead)hashtable[hashValue];
            if (nodeListHead == null)
            {
                hashtable[hashValue] = new XmlDiffNodeListHead(new XmlDiffNodeListMember(node, null));
            }
            else
            {
                XmlDiffNodeListMember newMember = new XmlDiffNodeListMember(node, null);
                nodeListHead.last.next = newMember;
                nodeListHead.last = newMember;
            }
        }

        #region Remove matching nodes from Hashtable
        private XmlDiffNode LocateAndRemoveMatchingNode(Hashtable hashtable, XmlDiffNodeListHead nodeListHead, XmlDiffNode nodeToMatch)
        {
            Debug.Assert(hashtable != null);
            Debug.Assert(nodeListHead != null);

            // find matching node in the list
            XmlDiffNodeListMember nodeList = nodeListHead.first;
            XmlDiffNode node = nodeList.node;
            if (IdenticalSubtrees(node, nodeToMatch))
            {
                // remove the node itself
                if (nodeList.next == null)
                {
                    hashtable.Remove(node.HashValue);
                }
                else
                {
                    Debug.Assert(nodeListHead.first != nodeListHead.last);
                    nodeListHead.first = nodeList.next;
                }
                return node;
            }
            else
            {
                while (nodeList.next != null)
                {
                    if (IdenticalSubtrees(nodeList.node, nodeToMatch))
                    {
                        nodeList.next = nodeList.next.next;
                        if (nodeList.next == null)
                        {
                            nodeListHead.last = nodeList;
                        }
                        return node;
                    }
                }
                return null;
            }
        }

        private void RemoveAncestors(Hashtable hashtable, XmlDiffNode node)
        {
            XmlDiffNode curAncestorNode = node.parent;
            while (curAncestorNode != null)
            {
                if (!RemoveNode(hashtable, curAncestorNode))
                    break;
                curAncestorNode.isSomeDescendantMatches = true;
                curAncestorNode = curAncestorNode.parent;
            }
        }

        private void RemoveDescendants(Hashtable hashtable, XmlDiffNode parent)
        {
            if (!parent.isExpanded || !parent.HasChildNodes)
                return;

            XmlDiffNode curNode = parent.FirstChildNode;
            for (; ; )
            {
                Debug.Assert(curNode != null);
                if (curNode.isExpanded && curNode.HasChildNodes)
                {
                    curNode = ((XmlDiffParentNode)curNode).firstChildNode;
                    continue;
                }

                RemoveNode(hashtable, curNode);

            TryNext:
                if (curNode.nextSibling != null)
                {
                    curNode = curNode.nextSibling;
                    continue;
                }
                else if (curNode.parent != parent)
                {
                    curNode = curNode.parent;
                    goto TryNext;
                }
                else
                {
                    break;
                }
            }
        }

        private bool RemoveNode(Hashtable hashtable, XmlDiffNode node)
        {
            Debug.Assert(hashtable != null);
            Debug.Assert(node != null);

            XmlDiffNodeListHead xmlNodeListHead = (XmlDiffNodeListHead)hashtable[node.HashValue];
            if (xmlNodeListHead == null)
            {
                return false;
            }

            XmlDiffNodeListMember xmlNodeList = xmlNodeListHead.first;
            if (xmlNodeList.node == node)
            {
                if (xmlNodeList.next == null)
                {
                    hashtable.Remove(node.HashValue);
                }
                else
                {
                    Debug.Assert(xmlNodeListHead.first != xmlNodeListHead.last);
                    xmlNodeListHead.first = xmlNodeList.next;
                }
            }
            else
            {
                if (xmlNodeList.next == null)
                {
                    return false;
                }

                while (xmlNodeList.next.node != node)
                {
                    xmlNodeList = xmlNodeList.next;
                    if (xmlNodeList.next == null)
                    {
                        return false;
                    }
                }

                xmlNodeList.next = xmlNodeList.next.next;
                if (xmlNodeList.next == null)
                {
                    xmlNodeListHead.last = xmlNodeList;
                }
            }
            return true;
        }
        #endregion

        // Shrinks the interval of nodes in one or mode XmlDiffShrankNode instances;
        // The shrank interval can contain only adjacent nodes => the position of two adjacent nodes differs by 1.
        private void ShrinkNodeInterval(XmlDiffNode firstSourceNode, XmlDiffNode lastSourceNode, XmlDiffNode firstTargetNode, XmlDiffNode lastTargetNode)
        {
            XmlDiffNode sourcePreviousSibling = null;
            XmlDiffNode targetPreviousSibling = null;

            // IgnoreChildOrder -> the nodes has been sorted by name/value before comparing.
            // 'Unsort' the matching interval of nodes (=sort by node position) to
            // group adjacent nodes that can be shrank.
            if (IgnoreChildOrder && firstSourceNode != lastSourceNode)
            {
                Debug.Assert(firstTargetNode != lastTargetNode);

                SortNodesByPosition(ref firstSourceNode, ref lastSourceNode, ref sourcePreviousSibling);
                SortNodesByPosition(ref firstTargetNode, ref lastTargetNode, ref targetPreviousSibling);
            }

            // replace the interval by XmlDiffShrankNode instance
            XmlDiffShrankNode sourceShrankNode = ReplaceNodeIntervalWithShrankNode(firstSourceNode,
                                                                                    lastSourceNode,
                                                                                    sourcePreviousSibling);
            XmlDiffShrankNode targetShrankNode = ReplaceNodeIntervalWithShrankNode(firstTargetNode,
                                                                                    lastTargetNode,
                                                                                    targetPreviousSibling);

            sourceShrankNode.MatchingShrankNode = targetShrankNode;
            targetShrankNode.MatchingShrankNode = sourceShrankNode;
        }

        private bool NodeInHashTable(Hashtable hashtable, XmlDiffNode node)
        {
            XmlDiffNodeListHead nodeListHeader = (XmlDiffNodeListHead)hashtable[node.HashValue];

            if (nodeListHeader == null)
            {
                return false;
            }

            XmlDiffNodeListMember nodeList = nodeListHeader.first;
            while (nodeList != null)
            {
                if (nodeList.node == node)
                {
                    return true;
                }
                nodeList = nodeList.next;
            }
            return false;
        }

        private XmlDiffShrankNode ReplaceNodeIntervalWithShrankNode(XmlDiffNode firstNode, XmlDiffNode lastNode, XmlDiffNode previousSibling)
        {
            XmlDiffShrankNode shrankNode = new XmlDiffShrankNode(firstNode, lastNode);
            XmlDiffParentNode parent = firstNode.parent;

            // find previous sibling node
            if (previousSibling == null &&
                 firstNode != parent.firstChildNode)
            {
                previousSibling = parent.firstChildNode;
                while (previousSibling.nextSibling != firstNode)
                    previousSibling = previousSibling.nextSibling;
            }

            // insert shrank node
            if (previousSibling == null)
            {
                Debug.Assert(firstNode == parent.firstChildNode);

                shrankNode.nextSibling = parent.firstChildNode;
                parent.firstChildNode = shrankNode;
            }
            else
            {
                shrankNode.nextSibling = previousSibling.nextSibling;
                previousSibling.nextSibling = shrankNode;
            }
            shrankNode.parent = parent;

            // remove the node interval & count the total number of nodes
            XmlDiffNode tmpNode;
            int totalNodesCount = 0;
            do
            {
                tmpNode = shrankNode.nextSibling;
                totalNodesCount += tmpNode.NodesCount;
                shrankNode.nextSibling = shrankNode.nextSibling.nextSibling;

            } while (tmpNode != lastNode);

            // adjust nodes count
            Debug.Assert(totalNodesCount > 0);
            if (totalNodesCount > 1)
            {
                totalNodesCount--;
                while (parent != null)
                {
                    parent.NodesCount -= totalNodesCount;
                    parent = parent.parent;
                }
            }

            return shrankNode;
        }

        #region Differencing algorithms
        private Diffgram WalkTreeAlgorithm()
        {
            // TODO

            return null;
        }

        private Diffgram ZhangShashaAlgorithm()
        {
            //// TODO
            //return null;

            // Pre-process the trees for the tree-to-tree comparison algorithm and diffgram generation.
            // This includes post-order numbering of all nodes (the source and target nodes are stored
            // in post-order in the _sourceNodes and _targetNodes arrays).
            this.PreprocessTree(this.sourceDiffDoc, ref sourceNodes);
            this.PreprocessTree(this.targetDiffDoc, ref targetNodes);

            // Find minimal edit distance between the trees
            EditScript editScript = (new MinimalTreeDistance(this)).FindMinimalDistance();
            Debug.Assert(editScript != null);

            // Generate the diffgram
            Diffgram diffgram = new DiffgramGenerator(this).GenerateFromEditScript(editScript);

            return diffgram;
        }

        /// <summary>
        /// Pre-process the trees for the tree-to-tree comparison algorithm and diffgram generation.
        /// </summary>
        /// <param name="xmlDiffDoc"></param>
        /// <param name="postOrderArray"></param>
        private void PreprocessTree(XmlDiffDocument xmlDiffDoc, ref XmlDiffNode[] postOrderArray)
        {
            // allocate the array for post-ordered nodes.
            // The index 0 is not used; this is to have the consistent indexing of all arrays in the algorithm;
            postOrderArray = new XmlDiffNode[xmlDiffDoc.NodesCount + 1];
            postOrderArray[0] = null;

            // recursively process all nodes
            int index = 1;
            this.DFSProcessNode(xmlDiffDoc, ref postOrderArray, ref index);

            // root node is a 'key root' node
            xmlDiffDoc.isKeyRoot = true;

            Debug.Assert(index - 1 == xmlDiffDoc.NodesCount);
        }

        /// <summary>
        /// Depth-first traverse a tree/sub-tree and store the nodes in the post order recursively.
        /// </summary>
        /// <param name="node">The root of the tree/sub-tree to process</param>
        /// <param name="postOrderArray"></param>
        /// <param name="currentIndex"></param>
        private void DFSProcessNode(XmlDiffNode node, ref XmlDiffNode[] postOrderArray, ref int currentIndex)
        {
            // process children
            if (node.HasChildNodes)
            {
                Debug.Assert(node.FirstChildNode != null);

                XmlDiffNode curChild = node.FirstChildNode;
                curChild.isKeyRoot = false;

                while(true)
                {
                    this.DFSProcessNode(curChild, ref postOrderArray, ref currentIndex);
                    curChild = curChild.nextSibling;

                    // 'key root' node is the root node and each node that has a previous sibling node
                    if (curChild != null)
                    {
                        curChild.isKeyRoot = true;
                    }                        
                    else break;
                }

                // the leftmost leaf in the subtree rooted at 'node'
                node.Left = node.FirstChildNode.Left;
            }
            else
            {
                // the leftmost leaf in the subtree rooted at 'node'
                node.Left = currentIndex;

                // total number of nodes in the subtree rooted at 'node'
                node.NodesCount = 1;
            }

            // put the node in post-order array
            Debug.Assert(postOrderArray.Length > currentIndex);
            postOrderArray[currentIndex++] = node;
        }
        #endregion

        #endregion

        #region Static Private Methods

        static private void SlowSortNodes(ref XmlDiffNode firstNode, ref XmlDiffNode lastNode, XmlDiffNode firstPreviousSibbling, XmlDiffNode lastNextSibling)
        {
            Debug.Assert(firstNode != null);
            Debug.Assert(lastNode != null);

            XmlDiffNode firstSortedNode = firstNode;
            XmlDiffNode lastSortedNode = firstNode;
            XmlDiffNode nodeToSort = firstNode.nextSibling;
            lastSortedNode.nextSibling = null;

            while (nodeToSort != null)
            {
                XmlDiffNode curNode = firstSortedNode;
                if (nodeToSort.Position < firstSortedNode.Position)
                {
                    XmlDiffNode tmpNode = nodeToSort.nextSibling;

                    nodeToSort.nextSibling = firstSortedNode;
                    firstSortedNode = nodeToSort;

                    nodeToSort = tmpNode;
                }
                else
                {
                    while (curNode.nextSibling != null &&
                            nodeToSort.Position > curNode.nextSibling.Position)
                        curNode = curNode.nextSibling;

                    XmlDiffNode tmpNode = nodeToSort.nextSibling;

                    if (curNode.nextSibling == null)
                        lastSortedNode = nodeToSort;

                    nodeToSort.nextSibling = curNode.nextSibling;
                    curNode.nextSibling = nodeToSort;

                    nodeToSort = tmpNode;
                }
            }

            // reconnect the sorted part in the tree
            if (firstPreviousSibbling == null)
                firstNode.parent.firstChildNode = firstSortedNode;
            else
                firstPreviousSibbling.nextSibling = firstSortedNode;

            lastSortedNode.nextSibling = lastNextSibling;

            // return
            firstNode = firstSortedNode;
            lastNode = lastSortedNode;
        }

        static private void QuickSortNodes(ref XmlDiffNode firstNode, ref XmlDiffNode lastNode, int count, XmlDiffNode firstPreviousSibbling, XmlDiffNode lastNextSibling)
        {
            Debug.Assert(count >= MininumNodesForQuicksort);
            Debug.Assert(MininumNodesForQuicksort >= 2);

            // allocate & fill in the array
            XmlDiffNode[] sortArray = new XmlDiffNode[count];
            {
                XmlDiffNode curNode = firstNode;
                for (int i = 0; i < count; i++, curNode = curNode.nextSibling)
                {
                    Debug.Assert(curNode != null);
                    sortArray[i] = curNode;
                }
            }

            // sort
            QuickSortNodesRecursion(ref sortArray, 0, count - 1);

            // link the nodes
            for (int i = 0; i < count - 1; i++)
                sortArray[i].nextSibling = sortArray[i + 1];

            if (firstPreviousSibbling == null)
                firstNode.parent.firstChildNode = sortArray[0];
            else
                firstPreviousSibbling.nextSibling = sortArray[0];

            sortArray[count - 1].nextSibling = lastNextSibling;

            // return
            firstNode = sortArray[0];
            lastNode = sortArray[count - 1];
        }

        static private void QuickSortNodesRecursion(ref XmlDiffNode[] sortArray, int firstIndex, int lastIndex)
        {
            Debug.Assert(firstIndex < lastIndex);

            int pivotPosition = sortArray[(firstIndex + lastIndex) / 2].Position;
            int i = firstIndex;
            int j = lastIndex;

            while (i < j)
            {
                while (sortArray[i].Position < pivotPosition) i++;
                while (sortArray[j].Position > pivotPosition) j--;

                if (i < j)
                {
                    XmlDiffNode tmpNode = sortArray[i];
                    sortArray[i] = sortArray[j];
                    sortArray[j] = tmpNode;
                    i++;
                    j--;
                }
                else if (i == j)
                {
                    i++;
                    j--;
                }
            }

            if (firstIndex < j)
                QuickSortNodesRecursion(ref sortArray, firstIndex, j);
            if (i < lastIndex)
                QuickSortNodesRecursion(ref sortArray, i, lastIndex);
        }
        #endregion

    }

    class XmlDiffNodeListHead
    {
        internal XmlDiffNodeListMember first;
        internal XmlDiffNodeListMember last;

        internal XmlDiffNodeListHead(XmlDiffNodeListMember firstMember)
        {
            Debug.Assert(firstMember != null);
            first = firstMember;
            last = firstMember;
        }
    }

    class XmlDiffNodeListMember
    {
        internal XmlDiffNode node;
        internal XmlDiffNodeListMember next;

        internal XmlDiffNodeListMember(XmlDiffNode node, XmlDiffNodeListMember next)
        {
            Debug.Assert(node != null);
            this.node = node;
            this.next = next;
        }
    }
}
