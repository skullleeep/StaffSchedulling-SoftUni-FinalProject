using StaffScheduling.Common;

namespace StaffScheduling.Tests.CommonTests
{
    public class HelperTests
    {
        private enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        private enum AnotherTestEnum
        {
            First,
            Second
        }

        private enum EmptyEnum
        {

        }

        [Test]
        public void EnumArray_ShouldReturnAllValuesForTestEnum_WhenEnumValid()
        {
            //Act
            var result = Helper.EnumArray<TestEnum>();

            //Assert
            Assert.That(result, Is.EqualTo(new[] { TestEnum.Value1, TestEnum.Value2, TestEnum.Value3 }));
        }

        [Test]
        public void EnumArray_ShouldReturnAllValuesForAnotherTestEnum_WhenEnumValid()
        {
            //Act
            var result = Helper.EnumArray<AnotherTestEnum>();

            //Assert
            Assert.That(result, Is.EqualTo(new[] { AnotherTestEnum.First, AnotherTestEnum.Second }));
        }

        [Test]
        public void EnumArray_ShouldReturnEmptyArray_WhenEnumEmpty()
        {

            //Act
            var result = Helper.EnumArray<EmptyEnum>();

            //Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void AddSpacesToString_ShouldReturnStringWithSpaces_WhenStringValid()
        {
            //Arrange
            string stringWithoutSpaces = "StringWithoutSpaces";
            string stringWithSpaces = "String Without Spaces";

            //Act
            string result = Helper.AddSpacesToString(stringWithoutSpaces);

            //Assert
            Assert.That(result, Is.EqualTo(stringWithSpaces));
        }

        [Test]
        public void AddSpacesToString_ShouldReturnSameString_WhenStringDoesNotNeedSpaces()
        {
            //Arrange
            string str = "Test";

            //Act
            string result = Helper.AddSpacesToString(str);

            //Assert
            Assert.That(result, Is.EqualTo(str));
        }

        [Test]
        public void AddSpacesToString_ShouldReturnEmpty_WhenStringIsEmpty()
        {
            //Arrange
            string str = string.Empty;

            //Act
            string result = Helper.AddSpacesToString(str);

            //Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
