<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kmeans.aspx.cs" Inherits="TR8Web.Kmeans.Kmeans" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>

        <div style="margin-top: 20px; margin-bottom: 10px">
        Step 1. 選擇資料檔
        </div>
        <asp:FileUpload ID="FileUpload1" runat="server"  />
        <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
        <div style="margin-top: 40px; margin-bottom: 10px">
        Step 2. 選擇計算目標
        </div>
        
        分群數 (K)&nbsp; <asp:TextBox ID="TextBox1" Text="4" runat="server" Width="30px" ControlToValidate="TextBox1"></asp:TextBox>
        &nbsp;
        <asp:Button ID="Button1" runat="server" Text="計算指定 K" OnClick="Button1_Click"/>
        &nbsp;<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="K: 2-99" ValidationExpression="\d{1,2}" ControlToValidate="TextBox1"></asp:RegularExpressionValidator>
        <br />
        <br />
        最大分組數 (K) <asp:TextBox ID="TextBox2" Text="30" runat="server" Width="30px" ControlToValidate="TextBox2"></asp:TextBox>
        &nbsp;
         
        <asp:Button ID="Button2" runat="server" Text="計算最適 K" OnClick="Button2_Click"/>
         &nbsp;<asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ErrorMessage="K &lt; 150" ValidationExpression="\d{1,3}" ControlToValidate="TextBox2"></asp:RegularExpressionValidator>
         
         <br />
        <br />
        <br />
        <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
    </div>  
        <br />

      
        <div style="margin:20px;">
            <asp:GridView ID="GridView3" HeaderStyle-BackColor="#3AC0F2" HeaderStyle-ForeColor="White"
    runat="server" AutoGenerateColumns="false">
    <Columns>
        <asp:BoundField DataField="Centroid" HeaderText="Centroid" ItemStyle-Width="50" />
        <asp:BoundField DataField="Vector" HeaderText="Init Vector" ItemStyle-Width="150" />
    </Columns>
</asp:GridView>
            <asp:Label ID="Label3" runat="server" Text=""></asp:Label>
        </div>
  
   <div style="float: left; margin: 20px;">

<asp:GridView ID="GridView2" HeaderStyle-BackColor="#3AC0F2" HeaderStyle-ForeColor="White"
    runat="server" AutoGenerateColumns="false">
    <Columns>
        <asp:BoundField DataField="Round" HeaderText="Round" ItemStyle-Width="30" />
        <asp:BoundField DataField="Distortion" HeaderText="Total Distortion" ItemStyle-Width="150" />

    </Columns>
</asp:GridView>
        </div>
        <div style="margin:40px;">
        <asp:GridView ID="GridView1" HeaderStyle-BackColor="#3AC0F2" HeaderStyle-ForeColor="White"
    runat="server" AutoGenerateColumns="false">
    <Columns>
        <asp:BoundField DataField="Round" HeaderText="Round" ItemStyle-Width="20" />
        <asp:BoundField DataField="Distortion" HeaderText="Distortion" ItemStyle-Width="150" />
        <asp:BoundField DataField="Group" HeaderText="Group" ItemStyle-Width="150" />
        <asp:BoundField DataField="Member" HeaderText="Member" ItemStyle-Width="400" />
        <asp:BoundField DataField="Size" HeaderText="Size" ItemStyle-Width="20" />
    </Columns>
</asp:GridView>
            </div>
    
    </form>
</body>
</html>
