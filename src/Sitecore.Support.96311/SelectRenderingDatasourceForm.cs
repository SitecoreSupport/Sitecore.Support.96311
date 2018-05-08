using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Support.Shell.Applications.Dialogs.SelectRenderingDatasource
{
    public class SelectRenderingDatasourceForm : Sitecore.Shell.Applications.Dialogs.SelectRenderingDatasource.SelectRenderingDatasourceForm
    {
        // Fields
        protected Edit ItemLink;
        protected Literal PathResolve;
        protected Border SearchOption;
        protected Border SearchSection;

        // Methods
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                if (!ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>().ItemBucketsEnabled())
                {
                    this.SearchOption.Visible = false;
                    this.SearchSection.Visible = false;
                }
                else
                {
                    this.SearchOption.Click = "ChangeMode(\"Search\")";
                    if (!string.IsNullOrEmpty(base.SelectDatasourceOptions.CurrentDatasource))
                    {
                        this.SetPathResolve();
                    }
                    this.SetSectionHeader();
                }
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if (!ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>().ItemBucketsEnabled())
            {
                base.OnOK(sender, args);
            }
            else
            {
                switch (this.CurrentMode)
                {
                    case "Clone":
                    case "Create":
                        base.OnOK(sender, args);
                        return;

                    case "Select":
                        {
                            Item selectionItem = base.Treeview.GetSelectionItem();
                            if (selectionItem != null)
                            {
                                Literal pathResolve = this.PathResolve;
                                if (pathResolve != null)
                                {
                                    pathResolve.Text = selectionItem.Paths.FullPath;
                                }
                                this.SetDialogResult(selectionItem);
                            }
                            else
                            {
                                this.SetDialogDataSourceResult(this.ItemLink.Value);
                            }
                            SheerResponse.CloseWindow();
                            return;
                        }
                    case "Search":
                        {
                            Item selectedItem = Context.ContentDatabase.GetItem(this.ItemLink.Value);
                            if (selectedItem != null)
                            {
                                Literal pathResolve = this.PathResolve;
                                if (pathResolve != null)
                                {
                                    pathResolve.Text = selectedItem.Paths.FullPath;
                                }
                                Item selectionItem = base.Treeview.GetSelectionItem();
                                if (selectionItem.TemplateID == Buckets.Util.Constants.SavedSearchTemplateID)
                                {
                                    this.SetDialogDataSourceResult(selectionItem.Fields[Buckets.Util.Constants.DefaultQuery].Value);
                                }
                                this.SetDialogResult(selectedItem);
                                SheerResponse.CloseWindow();
                            }
                            else
                            {
                                SheerResponse.Alert(Translate.Text("Please select an item from the results"), new string[0]);
                            }
                            break;
                        }
                }
            }
        }

        private void SetControlsForSearching(Item item)
        {
            base.OK.Disabled = false;
        }

        protected override void SetControlsOnModeChange()
        {
            base.SetControlsOnModeChange();
            if (ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>().ItemBucketsEnabled())
            {
                string currentMode = this.CurrentMode;
                if (currentMode != null)
                {
                    if (((currentMode == "Clone") || (currentMode == "Create")) || (currentMode == "Select"))
                    {
                        this.SearchSection.Visible = false;
                        this.SearchOption.Class = string.Empty;
                    }
                    else if (currentMode == "Search")
                    {
                        this.SearchOption.Class = "selected";
                        if (!base.CreateOption.Disabled)
                        {
                            base.CreateOption.Class = string.Empty;
                        }
                        base.CloneOption.Class = string.Empty;
                        base.SelectOption.Class = string.Empty;
                        base.SelectSection.Visible = false;
                        this.SearchSection.Visible = true;
                        base.CloneSection.Visible = false;
                        base.CreateSection.Visible = false;
                        this.SetControlsForSearching(base.CreateDestination.GetSelectionItem());
                        SheerResponse.Eval($"selectItemName('{base.NewDatasourceName.ID}')");
                    }
                }
                this.SetSectionHeader();
            }
        }

        protected virtual void SetPathResolve()
        {
            Item item = Context.Database.GetItem(base.SelectDatasourceOptions.CurrentDatasource);
            if (item != null)
            {
                Literal pathResolve = this.PathResolve;
                if (pathResolve != null)
                {
                    pathResolve.Text = pathResolve.Text + " " + item.Paths.FullPath;
                }
            }
        }

        private void SetSectionHeader()
        {
            if (this.CurrentMode == "Search")
            {
                base.SectionHeader.Text = Translate.Text("Search for content items");
            }
        }
    }
}