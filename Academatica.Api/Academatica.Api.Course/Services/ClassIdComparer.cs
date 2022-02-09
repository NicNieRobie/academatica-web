using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Academatica.Api.Course.Services
{
    public class ClassIdComparer : IComparer<string>
    {
        public ClassIdComparer() { }

        public int Compare(string x, string y)
        {
            Regex rgx = new Regex(@"^[0-9]+-[0-1]:[0-9]+-[0-9]+$");

            if (!rgx.IsMatch(x) || !rgx.IsMatch(y))
            {
                throw new ArgumentException("Invalid string format");
            }

            int xTierId = int.Parse(x.Split('-')[0]);
            int yTierId = int.Parse(y.Split('-')[0]);

            if (xTierId != yTierId)
            {
                return xTierId.CompareTo(yTierId);
            }

            string xTopicData = x.Split('-')[1];
            string yTopicData = y.Split('-')[1];

            int xSubjectMark = int.Parse(xTopicData.Split(':')[0]);
            int ySubjectMark = int.Parse(yTopicData.Split(':')[0]);

            if (xSubjectMark != ySubjectMark)
            {
                return xSubjectMark.CompareTo(ySubjectMark);
            }

            int xTopicNum = int.Parse(xTopicData.Split(':')[1]);
            int yTopicNum = int.Parse(yTopicData.Split(':')[1]);

            if (xTopicNum != yTopicNum)
            {
                return xTopicNum.CompareTo(yTopicNum);
            }

            int xClassId = int.Parse(x.Split('-')[2]);
            int yClassId = int.Parse(y.Split('-')[2]);

            return xClassId.CompareTo(yClassId);
        }
    }
}
