window.navigationHandler = {
  hasUnsavedChanges: false,
  setUnsavedChanges: function (value) {
    this.hasUnsavedChanges = value;
  },
  handleNavigation: function (dotNetHelper) {
    if (this.hasUnsavedChanges) {
      return new Promise((resolve) => {
        if (confirm("You have unsaved changes. Are you sure you want to leave?")) {
          this.hasUnsavedChanges = false;
          resolve(true);
        } else {
          resolve(false);
        }
      });
    }
    return Promise.resolve(true);
  }
};