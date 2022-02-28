using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizeTest.Shared.Enums
{
    public enum RolesEnum
    {
        [Description("-")] None,
        [Description("مدیر")] Admin,
        [Description("کاربر")] User,
        [Description("مشتری")] Customer,
        [Description("کارمند")] Employee,
        [Description("ویرایشگر")] Editor
    }
}
