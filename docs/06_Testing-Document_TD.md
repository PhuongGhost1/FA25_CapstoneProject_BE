CustomMapOSM - Testing Strategy

1. Testing Pyramid:
┌─────────────┐
│  20% E2E   │  - Playwright (browser tests)
├─────────────┤
│  30% Integ  │  - API integration tests
├─────────────┤
│  50% Unit   │  - xUnit/NUnit
└─────────────┘

2. Key Test Cases:
2.1 Map Creation:
- TC01: Create map with valid bounds
- TC02: Add/remove layers
- TC03: Save/Load map state

2.2 Export System:
- TC11: PDF export within quota
- TC12: PNG export at 300DPI
- TC13: Quota exceeded handling

2.3 Payment:
- TC21: Successful payment
- TC22: Failed payment retry
- TC23: Webhook verification

3. Automation:
- API Tests: Postman (Newman CLI)
- UI Tests: Playwright (TypeScript)
- Load Tests: Locust (Python)

4. Performance Testing:
- JMeter scenarios:
  • 100 concurrent map loads
  • 50 simultaneous exports
  • Peak payment processing

5. Security Testing:
- OWASP ZAP scans
- JWT token validation tests
- SQL injection protection