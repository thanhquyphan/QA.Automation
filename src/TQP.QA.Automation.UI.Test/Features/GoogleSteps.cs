using TechTalk.SpecFlow;
using TQP.QA.Automation.UI.Test.Pages;

namespace TQP.QA.Automation.UI.Test.Features
{
    [Binding]
    public class GoogleSteps
    {
        private readonly GooglePage _googlePage;

        public GoogleSteps(GooglePage googlePage)
        {
            _googlePage = googlePage;
        }

        [Given(@"I am navigated to Google")]
        public void GivenIAmNavigatedToGoogle()
        {
            _googlePage.Navigate();
        }
        
        [When(@"I search for `(.*)`")]
        public void WhenISearchFor(string query)
        {
            _googlePage.EnterSearchValue(query);
        }
        
        [When(@"I click search")]
        public void WhenIClickSearch()
        {
            _googlePage.Search();
        }
        
        [Then(@"the result page should have link `(.*)`")]
        public void ThenTheResultPageShouldHaveLink(string link)
        {
            _googlePage.SearchResultsContainsLink(link);
        }
    }
}
