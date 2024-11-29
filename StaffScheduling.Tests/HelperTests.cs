using StaffScheduling.Common;

namespace StaffScheduling.Tests
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
        public void EnumArray_ShouldReturnAllValuesForTestEnum()
        {
            //Act
            var result = Helper.EnumArray<TestEnum>();

            //Assert
            Assert.That(result, Is.EqualTo(new[] { TestEnum.Value1, TestEnum.Value2, TestEnum.Value3 }));
        }

        [Test]
        public void EnumArray_ShouldReturnAllValuesForAnotherTestEnum()
        {
            //Act
            var result = Helper.EnumArray<AnotherTestEnum>();

            //Assert
            Assert.That(result, Is.EqualTo(new[] { AnotherTestEnum.First, AnotherTestEnum.Second }));
        }

        [Test]
        public void EnumArray_ShouldHandleEmptyEnum()
        {

            //Act
            var result = Helper.EnumArray<EmptyEnum>();

            //Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void AddSpacesToString_ShouldReturnStringWithSpaces()
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
        public void AddSpacesToString_ShouldReturnSameStringIfNoNeedForSpaces()
        {
            //Arrange
            string str = "Test";

            //Act
            string result = Helper.AddSpacesToString(str);

            //Assert
            Assert.That(result, Is.EqualTo(str));
        }

        [Test]
        public void AddSpacesToString_ShouldReturnEmptyIfStartingStringIsEmpty()
        {
            //Arrange
            string str = String.Empty;

            //Act
            string result = Helper.AddSpacesToString(str);

            //Assert
            Assert.That(result, Is.EqualTo(String.Empty));
        }
    }
}
