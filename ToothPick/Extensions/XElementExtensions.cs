namespace ToothPick.Extensions
{
    public static class XElementExtensions
    {
        /// <summary>
        /// Creates nessecary parent nodes using the provided Queue, and assigns the value to the last child node.
        /// </summary>
        /// <param name="element">XElement to take action on</param>
        /// <param name="nodes">Queue of node names</param>
        /// <param name="value">Value for last child node</param>
        /// returns created/updated element        
        public static XElement UpdateOrCreateChildNode(this XElement element, Queue<string> nodes, string value)
        {
            int fullQueueCOunt = nodes.Count;
            for (int i = 0; i < fullQueueCOunt; i++)
            {
                string node = nodes.Dequeue();
                XElement? firstChildMatch = element.Elements(node).FirstOrDefault();
                if (firstChildMatch == null)
                {
                    XElement newChlid = new XElement(node);
                    element.Add(newChlid);
                    element = newChlid;
                }
                else
                    element = firstChildMatch;
            }

            element.Value = value;

            return element;
        }
    }
}
