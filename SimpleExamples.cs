using Eagle.Framework.Server.Entity;
using Eagle.Framework.Server.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sales.Payment
{
    public class TestNamingExample : TestBase
    {
        #region compute
        [Compute(SAStudent.pn.FullName, new string[] { SAStudent.pn.Name, SAStudent.pn.LastName }, UserSummary = "")]
        public void SAStudent_FullName_Compute(SAStudent entity, ComputeEventArgs<string> e)
        {
            e.NewValue = $"{entity.Name} {entity.LastName}";
        }

        /*
         * Use case: 
         * Name is not empty but LastName is empty
         * Name is empty but LastName is not empty 
         * Name and LastName is empty
         * Name and LastName is not empty
         */
        //UnitOfWork_StateUnderTest_ExpectedBehavior
        [TestMethod]
        public void SAStudentFullNameCompute_NameIsNotEmptyLastNameIsEmpty_ShouldBeEqualToName()
        {
            var student = new SAStudent(Context);
            student.Name = "Brent";
            student.LastName = string.Empty;
            Assert.AreEqual("Brent", student.FullName);
        }

        [TestMethod]
        public void SAStudentFullNameCompute_NameIsEmptyLastNameIsNotEmpty_ShouldBeEqualToLastName()
        {
            var student = new SAStudent(Context);
            student.Name = string.Empty;
            student.LastName = "Esh";
            Assert.AreEqual("Esh", student.FullName);
        }

        [TestMethod]
        public void SAStudentFullNameCompute_NameAndLastNameIsEmpty_ShouldBeEmpty()
        {
            var student = new SAStudent(Context);
            student.Name = string.Empty;
            student.LastName = string.Empty;
            Assert.AreEqual(string.Empty, student.FullName);
        }

        [TestMethod]
        public void SAStudentFullNameCompute_NameAndLastNameIsNotEmpty_ShouldBeEqualToNameAndLastName()
        {
            var student = new SAStudent(Context);
            student.Name = "Brent";
            student.LastName = "Esh";
            Assert.AreEqual("Brent Esh", student.FullName);
        }
        #endregion

        #region validation
        [Validate(SAStudent.pn.FullName, UserSummary = "")]
        [MessageTemplate(0, SeverityLevel.Error, "Full name is wrong", "Full name should not contain numeric number", "Please enter correct full name")]
        [MessageTemplate(1, SeverityLevel.Error, "Full name is wrong", "Full name length should not be grove then 100", "Please enter correct full name")]
        public void SAStudent_FullName_Validate(SAStudent entity, ValidateEventArgs<string> e)
        {
            var result = IsFullNameValid(e.NewValue);
            if (result >= 0)
            {
                entity.Message(e, (uint)result);
            }
        }

        static int IsFullNameValid(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return 0;

            if (fullName.Length > 100)
                return 1;

            return -1;
        }

        [TestMethod]
        public void IsFullNameValid_IsEmpty_Zero()
        {
            //TODO: ...
        }

        [TestMethod]
        public void IsFullNameValid_NotEmptyAndContaindsNumeric_One()
        {
            //TODO: ...
        }

        [TestMethod]
        public void IsFullNameValid_NotEmptyAndDoseNotContainsNumeric_NegativOne()
        {
            //TODO: ...
        }

        [TestMethod]
        public void SAStudentFulllNameValidate_NotValid_DisplayMessage()
        {
            //TODO: ...
        }

        [TestMethod]
        public void SAStudentFulllNameValidate_Valid_NoMessage()
        {
            //TODO: ...
        }
        #endregion
    }
}
