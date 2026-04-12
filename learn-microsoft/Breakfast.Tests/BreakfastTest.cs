using Breakfast.CL.Models;
using Breakfast.CL.Services;
using Moq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        

        [SetUp]
        public void Setup()
        {
            _coffeeService = new Mock<ICoffeeService>();
            _eggService = new Mock<IEggService>();
            _hashBrownService = new Mock<IHashBrownService>();
            _toastService = new Mock<IToastService>();
            _juiceService = new Mock<IJuiceService>();
        }

        [Test]
        [TestCase(2, 3, 2)]
        public void BreakfastMaker_Synchronous(int eggsCount, int hashBrownsCount, int toastSlices)
        {

            var sut = new BreakfastMaker(_coffeeService.Object, 
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         _toastService.Object, 
                                         _juiceService.Object);

            sut.BreakfastSynchronous(eggsCount, hashBrownsCount, toastSlices);

            _coffeeService.Verify(c => c.PourCoffee(), Times.Once());
            _eggService.Verify(c => c.FryEggs(eggsCount), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrowns(hashBrownsCount), Times.Once());
            _toastService.Verify(c => c.ToastBread(toastSlices), Times.Once());
            _toastService.Verify(c => c.ApplyButter(It.IsAny<Toast>()), Times.Once());
            _toastService.Verify(c => c.ApplyJam(It.IsAny<Toast>()), Times.Once());
            _juiceService.Verify(c => c.PourOJ(), Times.Once());
        }

        [Test]
        [TestCase(2, 3, 2)]
        public async Task BreakfastMakerAwait(int eggsCount, int hashBrownsCount, int toastSlices)
        {

            var sut = new BreakfastMaker(_coffeeService.Object,
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         _toastService.Object,
                                         _juiceService.Object);

            await sut.BreakfastAwait(eggsCount, hashBrownsCount, toastSlices);

            _coffeeService.Verify(c => c.PourCoffeeAsync(), Times.Once());
            _eggService.Verify(c => c.FryEggsAsync(eggsCount), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrownsAsync(hashBrownsCount), Times.Once());
            _toastService.Verify(c => c.ToastBreadAsync(toastSlices), Times.Once());
            _toastService.Verify(c => c.ApplyButter(It.IsAny<Toast>()), Times.Once());
            _toastService.Verify(c => c.ApplyJam(It.IsAny<Toast>()), Times.Once());
            _juiceService.Verify(c => c.PourOJAsync(), Times.Once());
        }

        [Test]
        [TestCase(2,3,2)]
        public async Task BreakfastMakerConcurrent(int eggsCount, int hashBrownsCount, int toastSlices)
        {

            var sut = new BreakfastMaker(_coffeeService.Object,
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         _toastService.Object,
                                         _juiceService.Object);

            await sut.BreakfastConcurrent(eggsCount, hashBrownsCount, toastSlices);

            _coffeeService.Verify(c => c.PourCoffeeAsync(), Times.Once());
            _eggService.Verify(c => c.FryEggsAsync(eggsCount), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrownsAsync(hashBrownsCount), Times.Once());
            _toastService.Verify(c => c.MakeToastWithButterAndJamAsync(toastSlices), Times.Once());
            _juiceService.Verify(c => c.PourOJAsync(), Times.Once());
        }

        [Test]
        [TestCase(2, 3, -1)]
        public async Task BreakfastMakerConcurrentException(int eggsCount, int hashBrownsCount, int toastSlices)
        {
            var realToastService = new ToastService();

            var sut = new BreakfastMaker(_coffeeService.Object,
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         realToastService,
                                         _juiceService.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.BreakfastConcurrent(eggsCount, hashBrownsCount, toastSlices));
           
            Assert.That(ex.Message, Is.EqualTo("The toaster is on fire"));

            _coffeeService.Verify(c => c.PourCoffeeAsync(), Times.Once());
            _eggService.Verify(c => c.FryEggsAsync(eggsCount), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrownsAsync(hashBrownsCount), Times.Once());
            _juiceService.Verify(c => c.PourOJAsync(), Times.Once());
        }

        [Test]
        [TestCase(2, 3, 2)]
        public async Task BreakfastMakerWhenAll(int eggsCount, int hashBrownsCount, int toastSlices)
        {

            var sut = new BreakfastMaker(_coffeeService.Object,
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         _toastService.Object,
                                         _juiceService.Object);

            await sut.BreakfastWhenAll(eggsCount, hashBrownsCount, toastSlices);

            _coffeeService.Verify(c => c.PourCoffeeAsync(), Times.Once());
            _eggService.Verify(c => c.FryEggsAsync(eggsCount), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrownsAsync(hashBrownsCount), Times.Once());
            _toastService.Verify(c => c.MakeToastWithButterAndJamAsync(toastSlices), Times.Once());
            _juiceService.Verify(c => c.PourOJAsync(), Times.Once());
        }

        [Test]
        [TestCase(2, 3, 2)]
        public async Task BreakfastMakerWhenAnyl(int eggsCount, int hashBrownsCount, int toastSlices)
        {

            var sut = new BreakfastMaker(_coffeeService.Object,
                                         _eggService.Object,
                                         _hashBrownService.Object,
                                         _toastService.Object,
                                         _juiceService.Object);

            await sut.BreakfastWhenAny(eggsCount, hashBrownsCount, toastSlices);

            _coffeeService.Verify(c => c.PourCoffeeAsync(), Times.Once());
            _eggService.Verify(c => c.FryEggsAsync(eggsCount), Times.Once());
            _hashBrownService.Verify(c => c.FryHashBrownsAsync(hashBrownsCount), Times.Once());
            _toastService.Verify(c => c.MakeToastWithButterAndJamAsync(toastSlices), Times.Once());
            _juiceService.Verify(c => c.PourOJAsync(), Times.Once());
        }
    }
}
