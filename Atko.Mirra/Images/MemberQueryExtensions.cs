using System.Collections.Generic;
using System.Linq;

namespace Atko.Mirra.Images
{
    public static class MemberQueryExtensions
    {
        /// <summary>
        /// Yield all static members in an enumerable of <see cref="MemberImage"/> instances.
        /// </summary>
        /// <param name="members">The members to filter.</param>
        /// <typeparam name="T">The type of <see cref="MemberImage"/> to filter.</typeparam>
        /// <returns>All static members.</returns>
        public static IEnumerable<T> Static<T>(this IEnumerable<T> members) where T : MemberImage
        {
            return members.Where((current) => current.IsStatic);
        }

        /// <summary>
        /// Yield all non-static members in an enumerable of <see cref="MemberImage"/> instances.
        /// </summary>
        /// <param name="members">The members to filter.</param>
        /// <typeparam name="T">The type of <see cref="MemberImage"/> to filter.</typeparam>
        /// <returns>All non-static members.</returns>
        public static IEnumerable<T> Instance<T>(this IEnumerable<T> members) where T : MemberImage
        {
            return members.Where((current) => !current.IsStatic);
        }

        /// <summary>
        /// Yield all public members in an enumerable of <see cref="MemberImage"/> instances.
        /// </summary>
        /// <param name="members">The members to filter.</param>
        /// <typeparam name="T">The type of <see cref="MemberImage"/> to filter.</typeparam>
        /// <returns>All public members.</returns>
        public static IEnumerable<T> Public<T>(this IEnumerable<T> members) where T : MemberImage
        {
            return members.Where((current) => current.IsPublic);
        }

        /// <summary>
        /// Yield all non-public members in an enumerable of <see cref="MemberImage"/> instances.
        /// </summary>
        /// <param name="members">The members to filter.</param>
        /// <typeparam name="T">The type of <see cref="MemberImage"/> to filter.</typeparam>
        /// <returns>All non-public members.</returns>
        public static IEnumerable<T> NonPublic<T>(this IEnumerable<T> members) where T : MemberImage
        {
            return members.Where((current) => !current.IsPublic);
        }
    }
}