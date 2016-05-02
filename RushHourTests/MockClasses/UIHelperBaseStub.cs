using System;
using ICities;

namespace RushHourTests.MockClasses
{
    class UIHelperBaseStub : UIHelperBase
    {
        public object AddButton(string text, OnButtonClicked eventCallback)
        {
            throw new NotImplementedException();
        }

        public object AddCheckbox(string text, bool defaultValue, OnCheckChanged eventCallback)
        {
            throw new NotImplementedException();
        }

        public object AddDropdown(string text, string[] options, int defaultSelection, OnDropdownSelectionChanged eventCallback)
        {
            throw new NotImplementedException();
        }

        public UIHelperBase AddGroup(string text)
        {
            throw new NotImplementedException();
        }

        public object AddSlider(string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
        {
            throw new NotImplementedException();
        }

        public object AddSpace(int height)
        {
            throw new NotImplementedException();
        }

        public object AddTextfield(string text, string defaultContent, OnTextChanged eventChangedCallback, OnTextSubmitted eventSubmittedCallback = null)
        {
            throw new NotImplementedException();
        }
    }
}
