using Bogus;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Faqs;
using CusomMapOSM_Application.Models.DTOs.Features.Faqs;
using CusomMapOSM_Domain.Entities.Faqs;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Faqs;
using CusomMapOSM_Infrastructure.Features.Faqs;
using FluentAssertions;
using Moq;
using Optional;
using Xunit;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Tests.Features.Faqs;

public class FaqServiceTests
{
    private readonly Mock<IFaqRepository> _mockFaqRepository;
    private readonly FaqService _faqService;
    private readonly Faker _faker;

    public FaqServiceTests()
    {
        _mockFaqRepository = new Mock<IFaqRepository>();
        _faqService = new FaqService(_mockFaqRepository.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task GetAllFaqsAsync_WithExistingFaqs_ShouldReturnFaqs()
    {
        // Arrange
        var faqs = new Faker<Faq>()
            .RuleFor(f => f.FaqId, f => f.Random.Int(1, 100))
            .RuleFor(f => f.Question, f => f.Lorem.Sentence())
            .RuleFor(f => f.Answer, f => f.Lorem.Paragraph())
            .RuleFor(f => f.Category, f => f.PickRandom("General", "Billing", "Technical"))
            .RuleFor(f => f.CreatedAt, f => f.Date.Past())
            .Generate(5);

        _mockFaqRepository.Setup(x => x.GetAllFaqsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(faqs);

        // Act
        var result = await _faqService.GetAllFaqsAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Faqs.Should().HaveCount(5);
        response.Faqs.Should().AllSatisfy(faq =>
        {
            faq.Question.Should().NotBeNullOrEmpty();
            faq.Answer.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetAllFaqsAsync_WithNoFaqs_ShouldReturnEmptyList()
    {
        // Arrange
        _mockFaqRepository.Setup(x => x.GetAllFaqsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Faq>());

        // Act
        var result = await _faqService.GetAllFaqsAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Faqs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllFaqsAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        _mockFaqRepository.Setup(x => x.GetAllFaqsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _faqService.GetAllFaqsAsync();

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    [Fact]
    public async Task GetFaqsByCategoryAsync_WithValidCategory_ShouldReturnFaqs()
    {
        // Arrange
        var category = "Billing";
        var faqs = new Faker<Faq>()
            .RuleFor(f => f.FaqId, f => f.Random.Int(1, 100))
            .RuleFor(f => f.Question, f => f.Lorem.Sentence())
            .RuleFor(f => f.Answer, f => f.Lorem.Paragraph())
            .RuleFor(f => f.Category, category)
            .RuleFor(f => f.CreatedAt, f => f.Date.Past())
            .Generate(3);

        _mockFaqRepository.Setup(x => x.GetFaqsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(faqs);

        // Act
        var result = await _faqService.GetFaqsByCategoryAsync(category);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Category.Should().Be(category);
        response.Faqs.Should().HaveCount(3);
        response.Faqs.Should().AllSatisfy(faq => faq.Category.Should().Be(category));
    }

    [Fact]
    public async Task GetFaqsByCategoryAsync_WithEmptyCategory_ShouldReturnError()
    {
        // Arrange
        var category = "";

        // Act
        var result = await _faqService.GetFaqsByCategoryAsync(category);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task GetFaqsByCategoryAsync_WithWhitespaceCategory_ShouldReturnError()
    {
        // Arrange
        var category = "   ";

        // Act
        var result = await _faqService.GetFaqsByCategoryAsync(category);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task GetFaqsByCategoryAsync_WithNonExistentCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var category = "NonExistent";
        _mockFaqRepository.Setup(x => x.GetFaqsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Faq>());

        // Act
        var result = await _faqService.GetFaqsByCategoryAsync(category);

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Faqs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFaqsByCategoryAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var category = "Billing";
        _mockFaqRepository.Setup(x => x.GetFaqsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _faqService.GetFaqsByCategoryAsync(category);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    [Fact]
    public async Task GetFaqByIdAsync_WithValidId_ShouldReturnFaq()
    {
        // Arrange
        var faqId = 1;
        var faq = new Faker<Faq>()
            .RuleFor(f => f.FaqId, faqId)
            .RuleFor(f => f.Question, f => f.Lorem.Sentence())
            .RuleFor(f => f.Answer, f => f.Lorem.Paragraph())
            .RuleFor(f => f.Category, f => f.PickRandom("General", "Billing"))
            .RuleFor(f => f.CreatedAt, f => f.Date.Past())
            .Generate();

        _mockFaqRepository.Setup(x => x.GetFaqByIdAsync(faqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(faq);

        // Act
        var result = await _faqService.GetFaqByIdAsync(faqId);

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Faq.Should().NotBeNull();
        response.Faq.FaqId.Should().Be(faqId);
        response.Faq.Question.Should().Be(faq.Question);
        response.Faq.Answer.Should().Be(faq.Answer);
    }

    [Fact]
    public async Task GetFaqByIdAsync_WithZeroId_ShouldReturnError()
    {
        // Arrange
        var faqId = 0;

        // Act
        var result = await _faqService.GetFaqByIdAsync(faqId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task GetFaqByIdAsync_WithNegativeId_ShouldReturnError()
    {
        // Arrange
        var faqId = -1;

        // Act
        var result = await _faqService.GetFaqByIdAsync(faqId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Validation)
        );
    }

    [Fact]
    public async Task GetFaqByIdAsync_WithNonExistentId_ShouldReturnError()
    {
        // Arrange
        var faqId = 999;
        _mockFaqRepository.Setup(x => x.GetFaqByIdAsync(faqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Faq?)null);

        // Act
        var result = await _faqService.GetFaqByIdAsync(faqId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.NotFound)
        );
    }

    [Fact]
    public async Task GetFaqByIdAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var faqId = 1;
        _mockFaqRepository.Setup(x => x.GetFaqByIdAsync(faqId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _faqService.GetFaqByIdAsync(faqId);

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }

    [Fact]
    public async Task GetFaqCategoriesAsync_WithExistingCategories_ShouldReturnCategories()
    {
        // Arrange
        var categories = new List<string> { "General", "Billing", "Technical", "Support" };
        _mockFaqRepository.Setup(x => x.GetFaqCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _faqService.GetFaqCategoriesAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        var response = result.ValueOrFailure();
        response.Categories.Should().HaveCount(4);
        response.Categories.Should().Contain("General");
        response.Categories.Should().Contain("Billing");
    }

    [Fact]
    public async Task GetFaqCategoriesAsync_WithNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        _mockFaqRepository.Setup(x => x.GetFaqCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _faqService.GetFaqCategoriesAsync();

        // Assert
        result.HasValue.Should().BeTrue();
        result.ValueOrFailure().Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFaqCategoriesAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        _mockFaqRepository.Setup(x => x.GetFaqCategoriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _faqService.GetFaqCategoriesAsync();

        // Assert
        result.HasValue.Should().BeFalse();
        result.Match(
            some: _ => Assert.Fail("Should not have succeeded"),
            none: error => error.Type.Should().Be(ErrorType.Failure)
        );
    }
}

