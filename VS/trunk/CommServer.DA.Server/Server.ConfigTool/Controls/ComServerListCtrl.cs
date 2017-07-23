using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

namespace Opc.ConfigTool
{
  public partial class ComServerListUserControl : BaseListUserControl
  {
    public ComServerListUserControl()
    {
      InitializeComponent();
      SetColumns(m_ColumnNames);
    }

    #region Private Fields
    /// <summary>
    /// The columns to display in the control.
    /// </summary>
    private readonly object[][] m_ColumnNames = new object[][]
    {
            new object[] { "ProgId",   HorizontalAlignment.Left, null },
            new object[] { "Codebase", HorizontalAlignment.Left, null }
    };

    private Guid m_catid;
    #endregion

    #region Public Interface
    /// <summary>
    /// Clears the contents of the control,
    /// </summary>
    public void Clear()
    {
      ItemsLV.Items.Clear();
      AdjustColumns();
    }

    /// <summary>
    /// Enumerates classes in category and populates this control.
    /// </summary>
    /// <param name="categoryId">The category identifier used byt the <see cref="ConfigUtils.EnumClassesInCategory"/> to enumerate the servers.</param>
    public void Initialize(Guid categoryId)
    {
      Clear();
      m_catid = categoryId;
      List<Guid> clsids = ConfigUtils.EnumClassesInCategory(categoryId);
      foreach (Guid clsid in clsids)
        AddItem(clsid);
      AdjustColumns();
    }
    #endregion

    #region Overridden Methods
    /// <see cref="BaseListUserControl.EnableMenuItems" />
    protected override void EnableMenuItems(ListViewItem clickedItem)
    {
      EditMI.Visible = m_catid == ConfigUtils.CATID_RegisteredDotNetOpcServers;
      EditMI.Enabled = ItemsLV.SelectedItems.Count == 1;
      DeleteMI.Enabled = ItemsLV.SelectedItems.Count > 0;
    }

    /// <see cref="BaseListUserControl.PickItems" />
    protected override void PickItems()
    {
      base.PickItems();

      if (ItemsLV.SelectedItems.Count == 1)
      {
        EditMI_Click(this, null);
        return;
      }
    }

    /// <see cref="BaseListUserControl.UpdateItem" />
    protected override void UpdateItem(ListViewItem listItem, object item)
    {
      if (!(item is Guid))
      {
        base.UpdateItem(listItem, item);
        return;
      }

      Guid clsid = (Guid)item;

      listItem.SubItems[0].Text = ConfigUtils.ProgIDFromCLSID(clsid);
      listItem.SubItems[1].Text = ConfigUtils.GetExecutablePath(clsid);

      listItem.Tag = item;

      if (m_catid == ConfigUtils.CATID_DotNetOpcServerWrappers)
      {
        listItem.ImageKey = "Folder";
      }

      else if (m_catid == ConfigUtils.CATID_RegisteredDotNetOpcServers)
      {
        listItem.ImageKey = "Method";
      }
      else
      {
        listItem.ImageKey = "Object";
      }
    }
    #endregion

    #region Event Handlers
    private void EditMI_Click(object sender, EventArgs e)
    {
      try
      {
        if (ItemsLV.SelectedItems.Count != 1)
        {
          return;
        }

        if (m_catid != ConfigUtils.CATID_RegisteredDotNetOpcServers)
        {
          return;
        }

        Guid clsid = (Guid)ItemsLV.SelectedItems[0].Tag;

        RegisteredDotNetOpcServer server = new RegisterServerDlg().ShowDialog(new RegisteredDotNetOpcServer(clsid));

        if (server != null)
        {
          UpdateItem(ItemsLV.SelectedItems[0], server.Clsid);
        }
      }
      catch (Exception exception)
      {
        GuiUtils.HandleException(this.Text, MethodBase.GetCurrentMethod(), exception);
      }
    }

    private void DeleteMI_Click(object sender, EventArgs e)
    {
      try
      {
        Guid[] clsids = base.GetSelectedItems(typeof(Guid)) as Guid[];

        if (clsids == null || clsids.Length == 0)
        {
          return;
        }

        for (int ii = 0; ii < clsids.Length; ii++)
        {
          ConfigUtils.UnregisterComServer(clsids[ii]);
        }

        List<ListViewItem> itemsToDelete = new List<ListViewItem>();

        foreach (ListViewItem item in ItemsLV.SelectedItems)
        {
          itemsToDelete.Add(item);
        }

        foreach (ListViewItem item in itemsToDelete)
        {
          item.Remove();
        }
      }
      catch (Exception exception)
      {
        GuiUtils.HandleException(this.Text, MethodBase.GetCurrentMethod(), exception);
      }
    }
    #endregion
  }
}

