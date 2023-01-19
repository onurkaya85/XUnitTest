using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.App;
using Xunit;

namespace XUnitTest.Test
{
    public class CalculatorTest
    {
        public Calculator calculator { get; set; }
        public Mock<ICalculatorService> mymock { get; set; }

        public CalculatorTest()
        {
            mymock = new Mock<ICalculatorService>();
            this.calculator = new Calculator(mymock.Object);
        }

        [Fact]
        public void AddTest()
        {
            //Arrange (Değişkenlerin initilaze edileceği yerdir.)
            int a = 5;
            int b = 20;

            //var calc = new Calculator();

            //Act (test edilecek metodun çalıştırılacağı yerdir)
            var total = calculator.Add(a, b);

            //Assert (Doğrulama evresidir)
            //Assert.Equal<int>(25, total);
            //Assert.NotEqual<int>(25, total);

            //Assert.Contains("Onur", "Onur Kaya");
            //Assert.DoesNotContain("OnurK", "Onur Kaya");

            var nameList = new List<string>()
            {
                "Onur","Emre","Fatih"
            };

            //Assert.Contains(nameList, v => v == "Ömer");
            //Assert.DoesNotContain(nameList, v => v == "Ömer");


            //Assert.True(5 < 2);
            //Assert.False(5 < 2);

            //Assert.True("".GetType() == typeof(int));

            var regex = "^dog";
            var regex2 = "^deneme";
            //Assert.Matches(regex, "dog ifadesi");
            //Assert.DoesNotMatch(regex2, "dog ifadesi");

            //Assert.StartsWith("Bir", "Bir masal");
            //Assert.EndsWith("masal", "Bir masal");

            //Assert.Empty(new List<string>());
            //Assert.NotEmpty(new List<string>() { "onur" });

            //Assert.InRange(10, 2, 20);
            //Assert.NotInRange(10, 2, 20);

            //Assert.Single(new List<string>() { "onur" });
            //Assert.Single<string>(new List<string>() { "onur" });

            //Assert.IsType<string>("onur");
            //Assert.IsNotType<string>(2);

            //Assert.IsAssignableFrom<IEnumerable<string>>(new List<string>());
            //Assert.IsAssignableFrom<object>("Onur");

            string deger = null;
            //Assert.Null(deger);
            //Assert.NotNull(deger);
        }

        //Test Metods Names
        //[MethodName_StateUnderTest_ExpectedBehavior]
        //Example => add_simpleValues_returnTotalValue

        [Theory]
        [InlineData(2, 5, 6)]
        [InlineData(10, 2, 12)]
        public void Add_SimpleValues_ReturnTotalValue(int a, int b, int total)
        {

            mymock.Setup(x => x.Add(a, b)).Returns(total);


            //var calc = new Calculator();
            var dTotal = calculator.Add(a, b);

            Assert.Equal<int>(total, dTotal);
            mymock.Verify(x => x.Add(a, b), Times.Once);
        }

        [Theory]
        [InlineData(3, 5, 15)]
        public void Multip_SimpleValue_ReturnsMultipedValue(int a, int b, int expectedValue)
        {
            //mymock.Setup(v => v.Multip(a, b)).Returns(expectedValue);
            int actualMultip = 0;
            mymock.Setup(v => v.Multip(It.IsAny<int>(), It.IsAny<int>())).Callback<int, int>((x, y) => actualMultip = x * y);

            calculator.Multip(a, b);
            Assert.Equal(expectedValue, actualMultip);


            calculator.Multip(5, 20);
            Assert.Equal(100, actualMultip);
        }

        [Theory]
        [InlineData(0, 5)]
        public void Multip_ZeroValue_ReturnsException(int a, int b)
        {
            mymock.Setup(v => v.Multip(a, b)).Throws(new Exception("a=0 olamaz"));

            Exception ex = Assert.Throws<Exception>(() => calculator.Multip(a, b));
            Assert.Equal("a=0 olamazz", ex.Message);

        }

        [Theory]
        [InlineData(0, 5, 0)]
        [InlineData(10, 0, 0)]
        public void Add_ZeroValues_ReturnZeroValue(int a, int b, int total)
        {
            //var calc = new Calculator();
            var dTotal = calculator.Add(a, b);

            Assert.Equal<int>(total, dTotal);
        }
    }
}
