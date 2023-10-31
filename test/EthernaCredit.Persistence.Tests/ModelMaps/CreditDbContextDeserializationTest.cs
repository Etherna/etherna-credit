//   Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.CreditSystem.Persistence.Helpers;
using Etherna.DomainEvents;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.MongODM.Core.Utility;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Etherna.CreditSystem.Persistence.ModelMaps
{
    public class CreditDbContextDeserializationTest
    {
        // Fields.
        private readonly CreditDbContext dbContext;
        private readonly Mock<IMongoDatabase> mongoDatabaseMock = new();
        
        // Constructor.
        public CreditDbContextDeserializationTest()
        {
            // Setup dbContext.
            var eventDispatcherMock = new Mock<IEventDispatcher>();
            var loggerMock = new Mock<ILogger<CreditDbContext>>();
            dbContext = new CreditDbContext(eventDispatcherMock.Object, loggerMock.Object);

            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);
        }

        // Data.
        public static IEnumerable<object[]> OperationLogDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<OperationLogBase>>();
                
                // "7fc7abe8-9a55-40a2-90ce-f3cba34bc005" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        $$"""
                          {
                              "_id" : ObjectId("652d9b14f66ea29b84305a23"),
                              "_m" : "7fc7abe8-9a55-40a2-90ce-f3cba34bc005",
                              "_t" : "DepositOperationLog",
                              "CreationDateTime" : ISODate("2023-10-26T20:20:15.830+0000"),
                              "Amount" : NumberDecimal("10.05"),
                              "Author" : "0x3D83bCF20E12Fb252D5e95b3fa0264B6221c4E79",
                              "User" : {
                                  "_m" : "b309c982-f30f-46ad-b076-c6030c8dbcd8",
                                  "_id" : ObjectId("652d9b0bf665611b84305e57")
                              }
                          }
                          """;

                    var expectedOpLogMock = new Mock<DepositOperationLog>();
                    expectedOpLogMock.Setup(l => l.Id).Returns("652d9b14f66ea29b84305a23");
                    expectedOpLogMock.Setup(l => l.CreationDateTime).Returns(new DateTime(2023, 10, 26, 20, 20, 15, 830));
                    expectedOpLogMock.Setup(l => l.Amount).Returns(10.05m);
                    expectedOpLogMock.Setup(l => l.Author).Returns("0x3D83bCF20E12Fb252D5e95b3fa0264B6221c4E79");
                    {
                        var userMock = new Mock<User>();
                        userMock.Setup(a => a.Id).Returns("652d9b0bf665611b84305e57");
                        expectedOpLogMock.Setup(c => c.User).Returns(userMock.Object);
                    }

                    tests.Add(new DeserializationTestElement<OperationLogBase>(sourceDocument, expectedOpLogMock.Object));
                }
                
                 // "74e021d4-6d86-4deb-b952-0c328839cfe2" - dev (pre v0.3.0), published for WAM event
                 {
                     var sourceDocument =
                         $$"""
                           {
                               "_id" : ObjectId("652d9b14f66ea29b84305a23"),
                               "_m" : "74e021d4-6d86-4deb-b952-0c328839cfe2",
                               "_t" : "UpdateOperationLog",
                               "CreationDateTime" : ISODate("2023-10-26T20:20:15.830+0000"),
                               "Amount" : NumberDecimal("-1.88E-8"),
                               "Author" : "ethernaGatewayCreditClientId",
                               "User" : {
                                   "_m" : "b309c982-f30f-46ad-b076-c6030c8dbcd8",
                                   "_id" : ObjectId("652d9b0bf665611b84305e57")
                               },
                               "Reason" : "DownloadFee"
                           }
                           """;

                     var expectedOpLogMock = new Mock<UpdateOperationLog>();
                     expectedOpLogMock.Setup(l => l.Id).Returns("652d9b14f66ea29b84305a23");
                     expectedOpLogMock.Setup(l => l.CreationDateTime).Returns(new DateTime(2023, 10, 26, 20, 20, 15, 830));
                     expectedOpLogMock.Setup(l => l.Amount).Returns(-0.0000000188m);
                     expectedOpLogMock.Setup(l => l.Author).Returns("ethernaGatewayCreditClientId");
                     {
                         var userMock = new Mock<User>();
                         userMock.Setup(a => a.Id).Returns("652d9b0bf665611b84305e57");
                         expectedOpLogMock.Setup(c => c.User).Returns(userMock.Object);
                     }
                     expectedOpLogMock.Setup(l => l.Reason).Returns("DownloadFee");

                     tests.Add(new DeserializationTestElement<OperationLogBase>(sourceDocument, expectedOpLogMock.Object));
                 }
                 
                 // "ba82b71f-1d41-45e2-a56b-d3293ea74c3a" - v0.3.9
                 {
                     var sourceDocument =
                         $$"""
                           {
                               "_id" : ObjectId("652d9b14f66ea29b84305a23"),
                               "_m" : "ba82b71f-1d41-45e2-a56b-d3293ea74c3a",
                               "_t" : "WelcomeCreditDepositOperationLog",
                               "CreationDateTime" : ISODate("2023-10-26T20:20:15.830+0000"),
                               "Amount" : NumberDecimal("0.1"),
                               "Author" : "0x3D83bCF20E12Fb252D5e95b3fa0264B6221c4E79",
                               "User" : {
                                   "_m" : "b309c982-f30f-46ad-b076-c6030c8dbcd8",
                                   "_id" : ObjectId("652d9b0bf665611b84305e57")
                               }
                           }
                           """;

                     var expectedOpLogMock = new Mock<WelcomeCreditDepositOperationLog>();
                     expectedOpLogMock.Setup(l => l.Id).Returns("652d9b14f66ea29b84305a23");
                     expectedOpLogMock.Setup(l => l.CreationDateTime).Returns(new DateTime(2023, 10, 26, 20, 20, 15, 830));
                     expectedOpLogMock.Setup(l => l.Amount).Returns(0.1m);
                     expectedOpLogMock.Setup(l => l.Author).Returns("0x3D83bCF20E12Fb252D5e95b3fa0264B6221c4E79");
                     {
                         var userMock = new Mock<User>();
                         userMock.Setup(a => a.Id).Returns("652d9b0bf665611b84305e57");
                         expectedOpLogMock.Setup(c => c.User).Returns(userMock.Object);
                     }

                     tests.Add(new DeserializationTestElement<OperationLogBase>(sourceDocument, expectedOpLogMock.Object));
                 }
                 
                 // "b0ffe059-c985-4f3d-8677-238ab9551ec3" - dev (pre v0.3.0), published for WAM event
                 {
                     var sourceDocument =
                         $$"""
                           {
                               "_id" : ObjectId("652d9b14f66ea29b84305a23"),
                               "_m" : "b0ffe059-c985-4f3d-8677-238ab9551ec3",
                               "_t" : "WithdrawOperationLog",
                               "CreationDateTime" : ISODate("2023-10-26T20:20:15.830+0000"),
                               "Amount" : NumberDecimal("-18"),
                               "Author" : "0x3D83bCF20E12Fb252D5e95b3fa0264B6221c4E79",
                               "User" : {
                                   "_m" : "b309c982-f30f-46ad-b076-c6030c8dbcd8",
                                   "_id" : ObjectId("652d9b0bf665611b84305e57")
                               }
                           }
                           """;

                     var expectedOpLogMock = new Mock<WithdrawOperationLog>();
                     expectedOpLogMock.Setup(l => l.Id).Returns("652d9b14f66ea29b84305a23");
                     expectedOpLogMock.Setup(l => l.CreationDateTime).Returns(new DateTime(2023, 10, 26, 20, 20, 15, 830));
                     expectedOpLogMock.Setup(l => l.Amount).Returns(-18m);
                     expectedOpLogMock.Setup(l => l.Author).Returns("0x3D83bCF20E12Fb252D5e95b3fa0264B6221c4E79");
                     {
                         var userMock = new Mock<User>();
                         userMock.Setup(a => a.Id).Returns("652d9b0bf665611b84305e57");
                         expectedOpLogMock.Setup(c => c.User).Returns(userMock.Object);
                     }

                     tests.Add(new DeserializationTestElement<OperationLogBase>(sourceDocument, expectedOpLogMock.Object));
                 }

                return tests.Select(t => new object[] { t });
            }
        }
        
         public static IEnumerable<object[]> UserBalanceDeserializationTests
         {
             get
             {
                 var tests = new List<DeserializationTestElement<UserBalance>>();
                 
                 // "873c5ee4-122b-4021-8dc9-524b9f50b73b" - dev (pre v0.3.0), published for WAM event
                 {
                     var sourceDocument =
                         $$"""
                           {
                               "_id" : ObjectId("652d9b14f66ea29b84305a23"),
                               "_m" : "873c5ee4-122b-4021-8dc9-524b9f50b73b",
                               "CreationDateTime" : ISODate("2023-10-16T20:27:17.837+0000"),
                               "Credit" : NumberDecimal("123.456"),
                               "User" : {
                                   "_m" : "b309c982-f30f-46ad-b076-c6030c8dbcd8",
                                   "_id" : ObjectId("652d9b0bf665611b84305e57")
                               }
                           }
                           """;

                     var expectedBalanceMock = new Mock<UserBalance>();
                     expectedBalanceMock.Setup(b => b.Id).Returns("652d9b14f66ea29b84305a23");
                     expectedBalanceMock.Setup(b => b.CreationDateTime).Returns(new DateTime(2023, 10, 16, 20, 27, 17, 837));
                     expectedBalanceMock.Setup(b => b.Credit).Returns(123.456m);
                     {
                         var userMock = new Mock<User>();
                         userMock.Setup(a => a.Id).Returns("652d9b0bf665611b84305e57");
                         expectedBalanceMock.Setup(c => c.User).Returns(userMock.Object);
                     }

                     tests.Add(new DeserializationTestElement<UserBalance>(sourceDocument, expectedBalanceMock.Object));
                 }

                 return tests.Select(t => new object[] { t });
             }
         }
         
         public static IEnumerable<object[]> UserDeserializationTests
         {
             get
             {
                 var tests = new List<DeserializationTestElement<User>>();
                 
                 // "0ff83163-b49f-4182-895d-bed59e73a976" - dev (pre v0.3.0), published for WAM event
                 {
                     var sourceDocument =
                         $$"""
                           {
                               "_id" : ObjectId("621d377299200245673f1071"),
                               "_m" : "0ff83163-b49f-4182-895d-bed59e73a976",
                               "CreationDateTime" : ISODate("2023-10-16T20:27:17.784+0000"),
                               "HasUnlimitedCredit" : true,
                               "SharedInfoId" : "652d9c8cea189ad4f2e7ce68"
                           }
                           """;

                     var expectedUserMock = new Mock<User>();
                     expectedUserMock.Setup(u => u.Id).Returns("621d377299200245673f1071");
                     expectedUserMock.Setup(u => u.CreationDateTime).Returns(new DateTime(2023, 10, 16, 20, 27, 17, 784));
                     expectedUserMock.Setup(u => u.HasUnlimitedCredit).Returns(true);
                     expectedUserMock.Setup(u => u.SharedInfoId).Returns("652d9c8cea189ad4f2e7ce68");

                     tests.Add(new DeserializationTestElement<User>(sourceDocument, expectedUserMock.Object));
                 }

                 return tests.Select(t => new object[] { t });
             }
         }

        // Tests.
        [Theory, MemberData(nameof(OperationLogDeserializationTests))]
        public void OperationLogDeserialization(DeserializationTestElement<OperationLogBase> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<OperationLogBase>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.Amount, result.Amount);
            Assert.Equal(testElement.ExpectedModel.Author, result.Author);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.User, result.User, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Author);
            Assert.NotNull(result.User);
            
            switch (testElement.ExpectedModel)
            {
                case DepositOperationLog _:
                    Assert.IsType<DepositOperationLog>(result);
                    break;
                case UpdateOperationLog expectedUpdateLog:
                    Assert.IsType<UpdateOperationLog>(result);
                    var resultUpdateLog = (UpdateOperationLog)result;
                    Assert.Equal(expectedUpdateLog.Reason, resultUpdateLog.Reason);
                    break;
                case WelcomeCreditDepositOperationLog _:
                    Assert.IsType<WelcomeCreditDepositOperationLog>(result);
                    break;
                case WithdrawOperationLog _:
                    Assert.IsType<WithdrawOperationLog>(result);
                    break;
                default: throw new InvalidOperationException();
            }
        }
        
        [Theory, MemberData(nameof(UserBalanceDeserializationTests))]
        public void UserBalanceDeserialization(DeserializationTestElement<UserBalance> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));
        
            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<UserBalance>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);
        
            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);
        
            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Credit, result.Credit);
            Assert.Equal(testElement.ExpectedModel.User, result.User, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.User);
        }
        
        [Theory, MemberData(nameof(UserDeserializationTests))]
        public void UserDeserialization(DeserializationTestElement<User> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));
        
            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<User>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);
        
            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);
        
            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.HasUnlimitedCredit, result.HasUnlimitedCredit);
            Assert.Equal(testElement.ExpectedModel.SharedInfoId, result.SharedInfoId);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.SharedInfoId);
        }
    }
}