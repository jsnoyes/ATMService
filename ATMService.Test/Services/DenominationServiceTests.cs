using ATMService.Core.Common;
using ATMService.Core.DataAccess;
using ATMService.Core.Models;
using ATMService.Core.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ATMService.Test.Services
{
    public class DenominationServiceTests
    {
        private readonly List<DenominationModel> _standardDenoms;
        private readonly List<HistoryModel> _loadedHistory;

        public DenominationServiceTests()
        {
            _standardDenoms = new List<DenominationModel> {
                new DenominationModel { Denomination = 100, Count = 10 },
                new DenominationModel { Denomination = 50, Count = 10 },
                new DenominationModel { Denomination = 20, Count = 10 },
                new DenominationModel { Denomination = 10, Count = 10 },
                new DenominationModel { Denomination = 5, Count = 10 },
                new DenominationModel { Denomination = 1, Count = 10 },
            };
            _loadedHistory = new List<HistoryModel>
            {
                new HistoryModel{ TotalAmount = 123, ExecutionTime = new DateTime(2020, 1, 1, 1, 0, 0), IsSuccess = true},
                new HistoryModel{ TotalAmount = 456, ExecutionTime = new DateTime(2020, 1, 1, 1, 0 , 1), IsSuccess = true},
                new HistoryModel{ TotalAmount = 789, ExecutionTime = new DateTime(2020, 1, 1, 1, 0 , 2), IsSuccess = true},
            };
        }

        [Fact]
        public void GetHistories_ShouldGetAllHistories()
        {
            // Arrange
            var histories = _loadedHistory;
            var daoMock = new Mock<IJSonDao>();
            daoMock.Setup(d => d.Histories).Returns(histories);
                
            var denomService = new DenominationService(daoMock.Object);

            // Act
            var result = denomService.GetHistory();

            // Assert
            Assert.Equal(histories.Count(), result.Count());

            foreach (var history in histories)
            {
                Assert.Contains(history, result);
            }
        }

        [Fact]
        public void GetDenominations_ShouldGetAllDenominations()
        {
            // Arrange
            var denoms = _standardDenoms;
            var daoMock = new Mock<IJSonDao>();
            daoMock.Setup(d => d.Denominations).Returns(denoms);

            var denomService = new DenominationService(daoMock.Object);

            // Act
            var result = denomService.GetDenominations();

            // Assert
            Assert.Equal(denoms.Count(), result.Count());

            foreach (var denom in denoms)
            {
                Assert.Contains(denom, result);
            }
        }

        [Fact]
        public void Withdraw_ShouldWithdraw()
        {
            // Arrange
            var daoMock = new Mock<IJSonDao>();
            daoMock.Setup(d => d.Denominations).Returns(_standardDenoms);
            var hists = new List<HistoryModel>();
            daoMock.Setup(d => d.Histories).Returns(hists);
            daoMock.Setup(d => d.Add(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel a) => hists.Add(a));

            var denomService = new DenominationService(daoMock.Object);

            var requestedAmount = 186; // 1 of each denomination.

            // Act
            var result = denomService.Withdraw(requestedAmount);

            // Assert
            Assert.Equal(requestedAmount, result.TotalAmount);
            Assert.True(result.IsSuccess);

            var denoms = denomService.GetDenominations();
            foreach(var denom in denoms)
            {
                Assert.Equal(9, denom.Count);
            }

            var histories = denomService.GetHistory();
            Assert.Single(histories);

            var history = histories.First();
            Assert.Equal(requestedAmount, history.TotalAmount);
        }

        [Fact]
        public void Withdraw_ShouldNotWithdraw()
        {
            // Arrange
            var daoMock = new Mock<IJSonDao>();
            var emptyDenoms = new List<DenominationModel> {
                new DenominationModel { Denomination = 100, Count = 0 },
                new DenominationModel { Denomination = 50, Count = 0 },
                new DenominationModel { Denomination = 20, Count = 0 },
                new DenominationModel { Denomination = 10, Count = 0 },
                new DenominationModel { Denomination = 5, Count = 0 },
                new DenominationModel { Denomination = 1, Count = 0 },
            };
            daoMock.Setup(d => d.Denominations).Returns(emptyDenoms);
            var hists = new List<HistoryModel>();
            daoMock.Setup(d => d.Histories).Returns(hists);
            daoMock.Setup(d => d.Add(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel a) => hists.Add(a));

            var denomService = new DenominationService(daoMock.Object);

            var requestedAmount = 1; 

            // Act
            var result = denomService.Withdraw(requestedAmount);

            // Assert
            Assert.Equal(requestedAmount, result.TotalAmount);
            Assert.False(result.IsSuccess);

            var denoms = denomService.GetDenominations();
            foreach (var denom in denoms)
            {
                Assert.Equal(0, denom.Count);
            }

            var histories = denomService.GetHistory();
            Assert.Single(histories);

            var history = histories.First();
            Assert.Equal(requestedAmount, history.TotalAmount);
        }

        [Fact]
        public void Restock_ShouldRestock()
        {
            // Arrange
            var daoMock = new Mock<IJSonDao>();
            daoMock.Setup(d => d.Denominations).Returns(_standardDenoms);

            var denomService = new DenominationService(daoMock.Object);

            var newStock = new List<DenominationModel>
            {
                new DenominationModel { Denomination = 100, Count = 10 },
                new DenominationModel { Denomination = 50, Count = 10 },
                new DenominationModel { Denomination = 20, Count = 10 },
                new DenominationModel { Denomination = 10, Count = 10 },
                new DenominationModel { Denomination = 5, Count = 10 },
                new DenominationModel { Denomination = 1, Count = 10 },
            };

            // Act
            var result = denomService.Restock(newStock);

            // Assert
            // Returned value.
            Assert.Equal(6, result.Count());
            foreach(var denom in result)
            {
                Assert.Equal(20, denom.Count);
            }

            // Value from datastore.
            var storedDenoms = denomService.GetDenominations().ToList();
            Assert.Equal(6, storedDenoms.Count());
            foreach (var denom in storedDenoms)
            {
                Assert.Equal(20, denom.Count);
            }
        }

        [Fact]
        public void Restock_ShouldNotRestockInvalidDenominations()
        {
            // Arrange
            var daoMock = new Mock<IJSonDao>();
            daoMock.Setup(d => d.Denominations).Returns(_standardDenoms);

            // Should NOT attempt to update datastore if can't restock.
            daoMock.Verify(d => d.Update(It.IsAny<List<DenominationModel>>()), Times.Never);

            var denomService = new DenominationService(daoMock.Object);

            var newStock = new List<DenominationModel>
            {
                new DenominationModel { Denomination = 9999, Count = 10 }
            };

            // Act
            // Assert
            Assert.Throws<InvalidAtmOperationException>(() => denomService.Restock(newStock));
        }

        [Fact]
        public void Restock_ShouldNotRestockNegativeCounts()
        {
            // Arrange
            var daoMock = new Mock<IJSonDao>();
            daoMock.Setup(d => d.Denominations).Returns(_standardDenoms);

            // Should NOT attempt to update datastore if can't restock.
            daoMock.Verify(d => d.Update(It.IsAny<List<DenominationModel>>()), Times.Never);

            var denomService = new DenominationService(daoMock.Object);

            var newStock = new List<DenominationModel>
            {
                new DenominationModel { Denomination = 100, Count = -20 }
            };

            // Act
            // Assert
            Assert.Throws<InvalidAtmOperationException>(() => denomService.Restock(newStock));
        }
    }
}
