public class TestNamingExample : TestBase
{
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
	public void SAStudent_FullNameCompute_NameIsEmptyLastNameIsNotEmpty_ShouldBeEqualToLastName()
	{
	var student = new SAStudent(Context);
	student.Name = string.Empty;
	student.LastName = "Esh";
	Assert.AreEqual("Esh", student.FullName);
	}

	[TestMethod]
	public void SAStudent_FullNameCompute_NameAndLastNameIsEmpty_ShouldBeEmpty()
	{
	var student = new SAStudent(Context);
	student.Name = string.Empty;
	student.LastName = string.Empty;
	Assert.AreEqual(string.Empty, student.FullName);
	}

	[TestMethod]
	public void SAStudent_FullNameCompute_NameAndLastNameIsNotEmpty_ShouldBeEqualToNameAndLastName()
	{
	var student = new SAStudent(Context);
	student.Name = "Brent";
	student.LastName = "Esh";
	Assert.AreEqual("Brent Esh", student.FullName);
	}

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
	if (IsEmpty(fullName))
	{
	return 0;
	}

	if (ContainsNumeric(fullName))
	{
	return 1;
	}


	return -1;

	bool ContainsNumeric(string value)
	{
	return true;
	}

	bool IsEmpty(string value)
	{
	return string.IsNullOrEmpty(value);
	}
	}

	[TestMethod]
	public void IsFullNameValid_IsEmpty_Zero()
	{

	}

	[TestMethod]
	public void IsFullNameValid_NotEmptyAndContaindsNumeric_One()
	{

	}

	[TestMethod]
	public void IsFullNameValid_NotEmptyAndDoseNotContainsNumeric_NegativOne()
	{

	}

	[TestMethod]
	public void SAStudentFulllNameValidate_NotValid_DisplayMessage()
	{

	}

	[TestMethod]
	public void SAStudentFulllNameValidate_Valid_NoMessage()
	{

	}

}
