using System;
using System.Collections.Generic;

namespace Academatica.Api.Course.Services
{
    public class TierIdComparer : IComparer<string>
    {
        public TierIdComparer() { }

        public int Compare(string x, string y)
        {
            int xNumContainer, yNumContainer;
            bool xIsNum = int.TryParse(x, out xNumContainer);
            bool yIsNum = int.TryParse(y, out yNumContainer);

            if (xIsNum && yIsNum)
            {
                return xNumContainer.CompareTo(yNumContainer);
            } else
            {
                return 0;
            }
        }
    }
}
