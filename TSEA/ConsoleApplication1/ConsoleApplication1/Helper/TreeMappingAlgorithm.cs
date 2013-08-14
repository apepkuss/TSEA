using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sam.XmlDiffPath
{
    internal class MinimalTreeDistance
    {
        /// <summary>
        /// MinimalTreeDistanceAlgo.Distance
        /// </summary>
        private struct Distance
        {
            internal int cost;
            internal EditScript editScript;
        }

        #region Fields
        // XmlDiff
        XmlDiff xmlDiff;

        // nodes in the post-order numbering - cached from XmlDiff 
        XmlDiffNode[] sourceNodes;
        XmlDiffNode[] targetNodes;

        // distances between all possible pairs of subtrees
        Distance[,] treeDistance;

        // distances between all possible pairs of forests - used by ComputeTreeDistance
        // method. It is hold here and allocated just once in the FindMinimalDistance method
        // instead of allocating it as a local variable in the ComputeTreeDistance method.
        Distance[,] forestDistance;

        // Static fields
        static readonly EditScriptEmpty EmptyEditScript = new EditScriptEmpty();

        // Operation costs
        internal static readonly int[] OperationCost = {
            0,              // Match                      = 0,
            4,              // Add                        = 1,
            4,              // Remove                     = 2,
            1,              // ChangeElementName          = 3,
            1,              // ChangeElementAttr1         = 4,
            2,              // ChangeElementAttr2         = 5,
	        3,              // ChangeElementAttr3         = 6,
	        2,              // ChangeElementNameAndAttr1  = 7,
	        3,              // ChangeElementNameAndAttr2  = 8,
	        4,              // ChangeElementNameAndAttr3  = 9,
            4,              // ChangePI                   = 10,
            4,              // ChangeER                   = 11,
            4,              // ChangeCharacterData        = 12,
            4,              // ChangeXmlDeclaration       = 13,
            4,              // ChangeDTD                  = 14,
            int.MaxValue/2, // Undefined                  = 15,
    };
        #endregion

        #region Constructor
        internal MinimalTreeDistance(XmlDiff xmlDiff)
        {
            Debug.Assert(OperationCost.Length - 1 == (int)XmlDiffOperation.Undefined,
                          "Correct the OperationCost array so that it reflects the XmlDiffOperation enum.");

            Debug.Assert(xmlDiff != null);
            this.xmlDiff = xmlDiff;
        }
        #endregion

        #region Internal Methods
        internal EditScript FindMinimalDistance()
        {
            EditScript resultEditScript = null;

            try
            {
                // cache sourceNodes and targetNodes arrays
                this.sourceNodes = xmlDiff.sourceNodes;
                this.targetNodes = xmlDiff.targetNodes;

                // create the treeDistance array - it contains distances between subtrees.
                // The zero-indexed row and column are not used.
                // This is to have the consistent indexing of all arrays in the algorithm;
                // forestDistance array requires 0-indexed border fields for recording the distance 
                // of empty forest.
                this.treeDistance = new Distance[sourceNodes.Length, targetNodes.Length];

                // create forestDistance array;
                // Parts of this array are independently used in subsequent calls of ComputeTreeDistance.
                // The array is allocated just once here in the biggest bounds it will ever need
                // instead of allocating it in each call of ComputeTreeDistance as a local variable.
                forestDistance = new Distance[sourceNodes.Length, targetNodes.Length];

                #region Compute the treeDistance array

                int i, j;
                for (i = 1; i < sourceNodes.Length; i++)
                {
                    if (sourceNodes[i].IsKeyRoot)
                    {
                        for (j = 1; j < targetNodes.Length; j++)
                        {
                            if (targetNodes[j].IsKeyRoot)
                            {
                                this.ComputeTreeDistance(i, j);
                            }
                        }
                    }
                }

                #endregion

                // get the result edit script
                resultEditScript = treeDistance[sourceNodes.Length - 1, targetNodes.Length - 1].editScript;
            }
            finally
            {
                forestDistance = null;
                treeDistance = null;
                sourceNodes = null;
                targetNodes = null;
            }

            // normalize the found edit script (expands script references etc.)
            return NormalizeScript(resultEditScript);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Computes the distance between two trees.
        /// </summary>
        /// <param name="sourcePos"></param>
        /// <param name="targetPos"></param>
        private void ComputeTreeDistance(int sourcePos, int targetPos)
        {
            int sourcePosLeft = sourceNodes[sourcePos].Left;
            int targetPosLeft = targetNodes[targetPos].Left;
            int i, j;

            // initialize borders of forestDistance array
            EditScriptAddOpened esAdd = new EditScriptAddOpened(targetPosLeft, EmptyEditScript);
            EditScriptRemoveOpened esRemove = new EditScriptRemoveOpened(sourcePosLeft, EmptyEditScript);

            forestDistance[sourcePosLeft - 1, targetPosLeft - 1].cost = 0;
            forestDistance[sourcePosLeft - 1, targetPosLeft - 1].editScript = EmptyEditScript;

            for (i = sourcePosLeft; i <= sourcePos; i++)
            {
                forestDistance[i, targetPosLeft - 1].cost = (i - sourcePosLeft + 1) * OperationCost[(int)XmlDiffOperation.Remove];
                forestDistance[i, targetPosLeft - 1].editScript = esRemove;
            }

            for (j = targetPosLeft; j <= targetPos; j++)
            {
                forestDistance[sourcePosLeft - 1, j].cost = (j - targetPosLeft + 1) * OperationCost[(int)XmlDiffOperation.Add];
                forestDistance[sourcePosLeft - 1, j].editScript = esAdd;
            }

            // compute the inside of forestDistance array
            for (i = sourcePosLeft; i <= sourcePos; i++)
            {
                for (j = targetPosLeft; j <= targetPos; j++)
                {
                    int sourceCurLeft = sourceNodes[i].Left;
                    int targetCurLeft = targetNodes[j].Left;

                    int removeCost = forestDistance[i - 1, j].cost + OperationCost[(int)XmlDiffOperation.Remove];
                    int addCost = forestDistance[i, j - 1].cost + OperationCost[(int)XmlDiffOperation.Add];

                    if (sourceCurLeft == sourcePosLeft && targetCurLeft == targetPosLeft)
                    {
                        XmlDiffOperation changeOp = sourceNodes[i].GetDiffOperation(targetNodes[j], xmlDiff);

                        Debug.Assert(XmlDiff.IsChangeOperation(changeOp) ||
                                      changeOp == XmlDiffOperation.Match ||
                                      changeOp == XmlDiffOperation.Undefined);

                        if (changeOp == XmlDiffOperation.Match)
                        {
                            // identical nodes matched
                            OpNodesMatch(i, j);
                        }
                        else
                        {
                            int changeCost = forestDistance[i - 1, j - 1].cost + OperationCost[(int)changeOp];

                            if (changeCost < addCost)
                            {
                                // operation 'change'
                                if (changeCost < removeCost)
                                    OpChange(i, j, changeOp, changeCost);
                                // operation 'remove'
                                else
                                    OpRemove(i, j, removeCost);
                            }
                            else
                            {
                                // operation 'add'
                                if (addCost < removeCost)
                                    OpAdd(i, j, addCost);
                                // operation 'remove'
                                else
                                    OpRemove(i, j, removeCost);
                            }
                        }

                        treeDistance[i, j].cost = forestDistance[i, j].cost;
                        treeDistance[i, j].editScript = forestDistance[i, j].editScript.GetClosedScript(i, j); ;
                    }
                    else
                    {
                        int m = sourceCurLeft - 1;
                        int n = targetCurLeft - 1;

                        if (m < sourcePosLeft - 1) m = sourcePosLeft - 1;
                        if (n < targetPosLeft - 1) n = targetPosLeft - 1;

                        // cost of concatenating of the two edit scripts
                        int compoundEditCost = forestDistance[m, n].cost + treeDistance[i, j].cost;

                        if (compoundEditCost < addCost)
                        {
                            if (compoundEditCost < removeCost)
                            {
                                // copy script
                                if (treeDistance[i, j].editScript == EmptyEditScript)
                                {
                                    Debug.Assert(treeDistance[i, j].cost == 0);
                                    OpCopyScript(i, j, m, n);
                                }
                                // concatenate scripts
                                else
                                    OpConcatScripts(i, j, m, n);
                            }
                            // operation 'remove'
                            else
                                OpRemove(i, j, removeCost);
                        }
                        else
                        {
                            // operation 'add'
                            if (addCost < removeCost)
                                OpAdd(i, j, addCost);
                            // operation 'remove'
                            else
                                OpRemove(i, j, removeCost);
                        }
                    }
                }
            }
        }

        private void OpChange(int i, int j, XmlDiffOperation changeOp, int cost)
        {
            forestDistance[i, j].editScript = new EditScriptChange(i, j, changeOp, forestDistance[i - 1, j - 1].editScript.GetClosedScript(i - 1, j - 1));
            forestDistance[i, j].cost = cost;
        }

        private void OpAdd(int i, int j, int cost)
        {
            EditScriptAddOpened openedAdd = forestDistance[i, j - 1].editScript as EditScriptAddOpened;

            if (openedAdd == null)
                openedAdd = new EditScriptAddOpened(j, forestDistance[i, j - 1].editScript.GetClosedScript(i, j - 1));

            forestDistance[i, j].editScript = openedAdd;
            forestDistance[i, j].cost = cost;
        }

        private void OpRemove(int i, int j, int cost)
        {
            EditScriptRemoveOpened openedRemove = forestDistance[i - 1, j].editScript as EditScriptRemoveOpened;

            if (openedRemove == null)
                openedRemove = new EditScriptRemoveOpened(i, forestDistance[i - 1, j].editScript.GetClosedScript(i - 1, j));

            forestDistance[i, j].editScript = openedRemove;
            forestDistance[i, j].cost = cost;
        }

        private void OpNodesMatch(int i, int j)
        {
            EditScriptMatchOpened openedMatch = forestDistance[i - 1, j - 1].editScript as EditScriptMatchOpened;

            if (openedMatch == null)
                openedMatch = new EditScriptMatchOpened(i, j, forestDistance[i - 1, j - 1].editScript.GetClosedScript(i - 1, j - 1));

            forestDistance[i, j].editScript = openedMatch;
            forestDistance[i, j].cost = forestDistance[i - 1, j - 1].cost;
        }

        private void OpCopyScript(int i, int j, int m, int n)
        {
            forestDistance[i, j].cost = forestDistance[m, n].cost;
            forestDistance[i, j].editScript = forestDistance[m, n].editScript.GetClosedScript(m, n);
        }

        private void OpConcatScripts(int i, int j, int m, int n)
        {
            forestDistance[i, j].editScript = new EditScriptReference(treeDistance[i, j].editScript, forestDistance[m, n].editScript.GetClosedScript(m, n));
            forestDistance[i, j].cost = treeDistance[i, j].cost + forestDistance[m, n].cost;

        }
        #endregion

        #region Static Private Methods
        /// <summary>
        /// expands 'reference edit script' items and removes the last item, which is the static instance of EmptyEditScript.
        /// </summary>
        /// <param name="es"></param>
        /// <returns></returns>
        static private EditScript NormalizeScript(EditScript es)
        {
            EditScript returnES = es;
            EditScript curES = es;
            EditScript prevES = null;

            while (curES != EmptyEditScript)
            {
                Debug.Assert(curES != null);

                if (curES.Operation != EditScriptOperation.EditScriptReference)
                {
                    prevES = curES;
                    curES = curES.nextEditScript;
                }
                else
                {
                    EditScriptReference refES = curES as EditScriptReference;

                    EditScript lastES = refES._editScriptReference;
                    Debug.Assert(lastES != EmptyEditScript && lastES != null);
                    while (lastES.Next != EmptyEditScript)
                    {
                        lastES = lastES.nextEditScript;
                        Debug.Assert(lastES != null);
                    }

                    lastES.nextEditScript = curES.nextEditScript;
                    curES = refES._editScriptReference;

                    if (prevES == null)
                        returnES = curES;
                    else
                        prevES.nextEditScript = curES;
                }
            }

            if (prevES != null)
                prevES.nextEditScript = null;
            else
                returnES = null;

            return returnES;
        }
        #endregion
    }
}
