using System;

using FluentAssertions;

using Gip.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gip.Tests.Core
{

    [TestClass]
    public class GibPadConstraintTests
    {

        [TestMethod]
        public void SameTypeShouldIntersect()
        {
            var cap1 = new GibCap(typeof(object), Array.Empty<GibCapConstraint>(), Array.Empty<GibCapFeature>());
            var cap2 = new GibCap(typeof(object), Array.Empty<GibCapConstraint>(), Array.Empty<GibCapFeature>());
            GibCap.CanIntersect(cap1, cap2).Should().BeTrue();
        }

    }

}
