# Organization API Tests

Thư mục này chứa các unit tests và integration tests toàn diện cho các Organization API endpoints trong dự án FA25_CusomMapOSM_BE.

## Cấu trúc Files

### 1. `OrganizationEndpointTests.cs`
**Mục đích**: Unit tests cơ bản cho tất cả Organization endpoints
**Bao gồm**:
- ✅ Create Organization (POST /organizations)
- ✅ Get All Organizations (GET /organizations)
- ✅ Get Organization By ID (GET /organizations/{id})
- ✅ Update Organization (PUT /organizations/{id})
- ✅ Delete Organization (DELETE /organizations/{id})
- ✅ Invite Member (POST /organizations/invite-member)
- ✅ Accept Invite (POST /organizations/accept-invite)
- ✅ Get My Invitations (GET /organizations/my-invitations)
- ✅ Get Organization Members (GET /organizations/{orgId}/members)
- ✅ Update Member Role (PUT /organizations/members/role)
- ✅ Remove Member (DELETE /organizations/members/remove)
- ✅ Get My Organizations (GET /organizations/my-organizations)
- ✅ Transfer Ownership (POST /organizations/transfer-ownership)
- ✅ Reject Invite (POST /organizations/reject-invite)
- ✅ Cancel Invite (POST /organizations/cancel-invite)

### 2. `OrganizationEndpointIntegrationTests.cs`
**Mục đích**: Integration tests và edge cases nâng cao
**Bao gồm**:
- 🔍 **Edge Cases**: Duplicate names, long descriptions, expired invitations
- 🔐 **Permission Tests**: Insufficient permissions, forbidden actions
- 📊 **Bulk Operations**: Large datasets, pagination scenarios
- ✅ **Data Validation**: Invalid inputs, email formats, role validation
- ⚠️ **Error Handling**: Not found, conflicts, business rule violations
- 🔄 **Concurrent Operations**: Already accepted/cancelled scenarios

### 3. `OrganizationTestDataHelper.cs`
**Mục đích**: Helper utilities để tạo test data
**Tính năng**:
- 🏭 **Factory Methods**: Tạo valid/invalid requests
- 📋 **Data Generators**: Sử dụng Bogus library để tạo fake data
- 🔧 **Reusable Components**: Có thể sử dụng lại trong nhiều test files
- 📊 **Response Builders**: Tạo response DTOs cho mocking

## Cách chạy Tests

### Chạy tất cả Organization tests:
```bash
dotnet test --filter "OrganizationEndpoint"
```

### Chạy specific test class:
```bash
dotnet test --filter "OrganizationEndpointTests"
dotnet test --filter "OrganizationEndpointIntegrationTests"
```

### Chạy specific test method:
```bash
dotnet test --filter "CreateOrganization_WithValidRequest_ShouldReturnSuccess"
```

### Chạy với verbose output:
```bash
dotnet test --filter "OrganizationEndpoint" --verbosity normal
```

## Test Coverage

### API Endpoints được test: **16/16** ✅
- [x] Create Organization
- [x] Get All Organizations  
- [x] Get Organization By ID
- [x] Update Organization
- [x] Delete Organization
- [x] Invite Member
- [x] Accept Invite
- [x] Get My Invitations
- [x] Get Organization Members
- [x] Update Member Role
- [x] Remove Member
- [x] Get My Organizations
- [x] Transfer Ownership
- [x] Reject Invite
- [x] Cancel Invite
- [x] Get Organization Members

### Test Scenarios được cover:
- ✅ **Happy Path**: Successful operations với valid data
- ✅ **Authentication**: Unauthorized access scenarios
- ✅ **Authorization**: Permission-based access control
- ✅ **Validation**: Invalid inputs và business rule violations
- ✅ **Error Handling**: Not found, conflicts, server errors
- ✅ **Edge Cases**: Boundary conditions và unusual scenarios
- ✅ **Data Integrity**: Duplicate prevention, referential integrity

## Test Patterns được sử dụng

### 1. **AAA Pattern** (Arrange-Act-Assert)
```csharp
// Arrange
var client = CreateAuthenticatedClient();
var request = CreateValidRequest();

// Act
var response = await client.PostAsJsonAsync("/endpoint", request);

// Assert
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

### 2. **Mocking với Moq**
```csharp
_mockOrganizationService.Setup(x => x.Create(request))
    .ReturnsAsync(Option.Some<OrganizationResDto, Error>(response));
```

### 3. **Fake Data Generation với Bogus**
```csharp
var request = new Faker<OrganizationReqDto>()
    .RuleFor(r => r.OrgName, f => f.Company.CompanyName())
    .Generate();
```

### 4. **FluentAssertions cho readable assertions**
```csharp
result.Should().NotBeNull();
result!.Organizations.Should().HaveCount(3);
```

## Dependencies

- **xUnit**: Test framework
- **Moq**: Mocking library
- **Bogus**: Fake data generation
- **FluentAssertions**: Assertion library
- **ASP.NET Core Test Host**: Integration testing

## Best Practices được follow

1. ✅ **Descriptive Test Names**: Tên test mô tả rõ ràng scenario
2. ✅ **Independent Tests**: Mỗi test độc lập, không phụ thuộc vào nhau
3. ✅ **Test Data Isolation**: Mỗi test tạo data riêng
4. ✅ **Comprehensive Coverage**: Cover cả happy path và edge cases
5. ✅ **Consistent Structure**: Cùng một pattern cho tất cả tests
6. ✅ **Helper Methods**: Tái sử dụng code thông qua helper classes

## Lưu ý cho Developer

### Khi thêm endpoint mới:
1. Thêm unit test vào `OrganizationEndpointTests.cs`
2. Thêm integration test vào `OrganizationEndpointIntegrationTests.cs`
3. Thêm helper methods vào `OrganizationTestDataHelper.cs`
4. Update README này

### Khi modify existing endpoint:
1. Update related tests
2. Ensure all tests still pass
3. Add new edge cases nếu cần

### Debug failing tests:
```bash
dotnet test --filter "TestMethodName" --verbosity diagnostic
```

## Future Enhancements

- [ ] Performance tests cho bulk operations
- [ ] Load testing cho concurrent scenarios
- [ ] Database integration tests với actual DB
- [ ] API contract testing với Pact
- [ ] Snapshot testing cho responses