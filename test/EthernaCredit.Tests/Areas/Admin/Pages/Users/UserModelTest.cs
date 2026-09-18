// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.Credit.Services.Domain;
using Etherna.SwarmSdk.Models;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Credit.Areas.Admin.Pages.Users
{
    public class UserModelTest
    {
        // Consts.
        private const string SharedInfoId = "6502fe5c1c5a4a4a9c6f0b1a";
        private const string UserId = "6502fe5c1c5a4a4a9c6f0b1b";

        // Tests.
        [Theory]
        [MemberData(nameof(PreviousAddresses))]
        public async Task OnGetAsync_WithAnyNumberOfPreviousAddresses_ListsThemOnePerLine(
            EthAddress[] previousAddresses,
            string expectedText)
        {
            // Arrange.
            var pageModel = BuildPageModel(previousAddresses);

            // Action.
            await pageModel.OnGetAsync(UserId);

            // Assert.
            Assert.Equal(expectedText, pageModel.EtherPreviousAddressesText);
        }

        // Test data.
        public static TheoryData<EthAddress[], string> PreviousAddresses()
        {
            EthAddress first = "0x1111111111111111111111111111111111111111";
            EthAddress second = "0x2222222222222222222222222222222222222222";

            return new TheoryData<EthAddress[], string>
            {
                //the shape of nearly every user
                { [], "" },
                { [first], first.ToString() },
                { [first, second], first.ToString() + '\n' + second.ToString() }
            };
        }

        // Helpers.
        private static UserModel BuildPageModel(EthAddress[] previousAddresses)
        {
            var userMock = new Mock<User>();
            userMock.Setup(u => u.Id).Returns(UserId);
            userMock.Setup(u => u.SharedInfoId).Returns(SharedInfoId);

            var userSharedInfoMock = new Mock<UserSharedInfo>();
            userSharedInfoMock.Setup(i => i.EtherAddress).Returns("0x3333333333333333333333333333333333333333");
            userSharedInfoMock.Setup(i => i.EtherPreviousAddresses).Returns(previousAddresses);

            var creditDbContextMock = new Mock<ICreditDbContext>();
            creditDbContextMock.Setup(c => c.Users.FindOneAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userMock.Object);

            var sharedDbContextMock = new Mock<ISharedDbContext>();
            sharedDbContextMock.Setup(c => c.UsersInfo.FindOneAsync(SharedInfoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userSharedInfoMock.Object);

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.GetUserBalanceAsync(userMock.Object))
                .ReturnsAsync(new XDaiValue(13.05m));

            return new UserModel(creditDbContextMock.Object, sharedDbContextMock.Object, userServiceMock.Object);
        }
    }
}
