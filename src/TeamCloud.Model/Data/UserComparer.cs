using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Model.Data
{
    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            if (object.ReferenceEquals(x, y))
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.Id == y.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(User obj)
            => obj.Id.GetHashCode();
    }
}
