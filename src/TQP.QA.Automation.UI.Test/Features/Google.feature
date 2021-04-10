Feature: Google
    Searching Google for JustSimplyCode blog

@mytag
Scenario: Search JustSimplyCode on Google
    Given I am navigated to Google
    When I search for `JustSimplyCode`
    And I click search
    Then the result page should have link `https://justsimplycode.com/`
