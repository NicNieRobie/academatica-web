using System;
using System.Collections.Generic;

namespace Academatica.Api.Course.Services
{
    public class TopicIdComparer : IComparer<string>
    {
        public TopicIdComparer() { }

        public int Compare(string x, string y)
        {
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

            return xTopicNum.CompareTo(yTopicNum);
        }
    }
}
