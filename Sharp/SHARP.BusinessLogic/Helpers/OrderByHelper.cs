using System;

namespace SHARP.BusinessLogic.Helpers
{
    public static class OrderByHelper
    {
        public static TFunc GetOrderBySelector<TEnum, TFunc>(string orderBy,
            Func<TEnum, TFunc> selectorFunc) where TEnum : struct where TFunc : class
        {
            return Enum.TryParse(orderBy, true, out TEnum orderByEnum) ? selectorFunc(orderByEnum) : null;
        }
    }
}
