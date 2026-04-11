using Breakfast.CL.Models;
using Breakfast.CL.Services;
using Moq;

namespace Breakfast.Tests
{
    [TestFixture]
    public class Tests
    {
        private Mock<ICoffeeService> _coffeeService;
        private Mock<IEggService> _eggService;
        private Mock<IHashBrownService> _hashBrownService;
        private Mock<IToastService> _toastService;
        private Mock<IJuiceService> _juiceService;
        

        [OneTimeSetUp]
        public void Setup()
        {
            _coffeeService = new Mock<ICoffeeService>();
            _eggService = new Mock<IEggService>();
            _hashBrownService = new Mock<IHashBrownService>();
            _toastService = new Mock<IToastService>();
            _juiceService = new Mock<IJuiceService>();
        }

        [Test]
        public void BreakfastMaker_Synchronous()
        {

            var sut = new BreakfastMaker(_coffeeService.Object, 
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         _toastService.Object, 
                                         _juiceService.Object);

            sut.Synchronous();

            _coffeeService.Verify(c => c.PourCoffee(), Times.Once());
            _eggService.Verify(c => c.FryEggs(2), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrowns(3), Times.Once());
            _toastService.Verify(c => c.ToastBread(2), Times.Once());
            _toastService.Verify(c => c.ApplyButter(It.IsAny<Toast>()), Times.Once());
            _toastService.Verify(c => c.ApplyJam(It.IsAny<Toast>()), Times.Once());
            _juiceService.Verify(c => c.PourOJ(), Times.Once());
        }
    }
}
