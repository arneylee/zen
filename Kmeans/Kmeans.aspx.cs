using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace TR8Web.Kmeans
{
    public partial class Kmeans : System.Web.UI.Page
    {
        private const string UPLOAD_FOLDER = "~/Upload";

        public class KmeansParam
        {
            public double[][] Distortion { get; set; }
            public double[] MaxDistortion { get; set; }
            public double[] MinDistortion { get; set; }
            public double[] AvgDistortion { get; set; }
            public int      MaxGroupNo { get; set; }
            public int      Run { get; set; }
            public int      BestK { get; set; }
            public int      GroupNoStart { get; set; }
            public int[]    GroupNo { get; set; }
            public int      RepeatTimes { get; set; }
            public int[]    BelongToCluster { get; set; }
            public string   FilePath { get; set; }
            public string   Delimiter { get; set; }
            public double[][] Record { get; set; }
            public double[][] InitCentroid { get; set; }
            public double[][] CurrentCentroid { get; set; }
            public double[][] UpdatedCentroid { get; set; }
            public double[,,] CentroidHistory { get; set; }
            public int      NumOfRows { get; set; }
            public int      NumOfCols { get; set; }

            public DataTable DetailsTable { get; set; }
            public DataTable SumTable { get; set; }
            public DataTable InitVectorTable { get; set; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            GridView1.DataSource = null;
            GridView1.DataBind();
            GridView2.DataSource = null;
            GridView2.DataBind();
            GridView3.DataSource = null;
            GridView3.DataBind();
            
            Label3.Text = string.Empty;
            Label2.Text = string.Empty;
            Label1.Text = string.Empty;
        }

        protected void StandardKmeans(ref KmeansParam Param)
        {
            int GroupNo = Param.MaxGroupNo;
            int i = 0;

            InitStatTable(ref Param);
            // k-means++
            PreProcessing(GroupNo, ref Param);
            GetInitVectorInfo(Param);
            do
            {
                RepeatAssignToGroup(GroupNo, ref Param);
                GetGroupStatus(++i, ref Param);
                UpdateCentroid(GroupNo, ref Param);
            } while (!RepeatStabled(GroupNo, ref Param));

        }

        private void GetInitVectorInfo(KmeansParam Param)
        {
            DataTable dt = Param.InitVectorTable;
            
            int dimension = Param.NumOfCols;
            int group = Param.MaxGroupNo;

            for (int i = 0; i < group; i++)
            {
                DataRow dr = dt.NewRow();
                dr["Centroid"] = i;
                for(int j = 0; j < dimension; j++)
                {
                    dr["Vector"] += Param.InitCentroid[i][j] + ", ";
                }
                dt.Rows.Add(dr);
            }
            
        }

        private void InitStatTable(ref KmeansParam Param)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Round", typeof(String));
            dt.Columns.Add("Distortion", typeof(Double));
            dt.Columns.Add("Group", typeof(String));
            dt.Columns.Add("Member", typeof(String));
            dt.Columns.Add("Size", typeof(String));
            Param.DetailsTable = dt;

            dt = new DataTable();
            dt.Columns.Add("Round", typeof(String));
            dt.Columns.Add("Distortion");
            Param.SumTable = dt;

            dt = new DataTable();
            dt.Columns.Add("Centroid", typeof(String));
            dt.Columns.Add("Vector", typeof(String));
            Param.InitVectorTable = dt;

        }

        private void GetGroupStatus(int run, ref KmeansParam Param)
        {
            int[] cluster = Param.BelongToCluster;
            int records = Param.NumOfRows;
            int GroupNo = Param.MaxGroupNo;
            int[] size = new int[GroupNo];
            double[][] DataRecord = Param.Record;
            double[][] centroid = Param.CurrentCentroid;
            double[] distortion = new double[GroupNo];
            string[] member = new string[GroupNo];
             
            Array.Clear(distortion, 0, distortion.Length);
            Array.Clear(size, 0, size.Length);
            

            for (int i = 0; i < records; i++)
            {
                for (int j = 0; j < GroupNo; j++)
                {
                    if (cluster[i] == j)
                    {
                        distortion[j] += Distance(DataRecord[i], centroid[j]);
                        member[j] += i + ", ";
                        size[j]++;
                    }
                }
            }

            for (int i = 0; i < GroupNo; i++)
            {
                DataTable dt = Param.DetailsTable;
                DataRow dr = dt.NewRow();
                dr["Round"] = run;
                dr["Distortion"] = distortion[i];
                dr["Member"] = member[i];
                dr["Size"] = size[i];

                for (int j = 0; j < Param.CurrentCentroid[i].Length; j++)
                {
                    dr["Group"] += Param.CurrentCentroid[i][j] + ", ";
                }
                dt.Rows.Add(dr);
            }

            DataTable dtt = Param.SumTable;
            DataRow dd = dtt.NewRow();
            dd["Round"] = run;
            dd["Distortion"] = distortion.Sum();
            dtt.Rows.Add(dd);

            
        }

       

        protected string GetUploadFolderPath()
        {
            return Server.MapPath(UPLOAD_FOLDER);
        }

        protected string GetFilePath(string fileName)
        {
            string realPath = GetUploadFolderPath();
            string filePath = Path.Combine(realPath, fileName);
            return filePath;
        }

        protected void ShowFiles()
        {
            string folderPath = GetUploadFolderPath();
            string[] files = Directory.GetFiles(folderPath);
            this.Label1.Text = string.Empty;
            for (int i = 0; i < files.Length; i++)
            {
                if (i != 0)
                {
                    this.Label1.Text += "<br />";
                }

                this.Label1.Text += Path.GetFileName(files[i]);
            }
            
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                KmeansParam Param = new KmeansParam();
                // 建立資料夾
                string realPath = GetUploadFolderPath();
                if (!Directory.Exists(realPath))
                {
                    Directory.CreateDirectory(realPath);
                }

                Param.FilePath = GetFilePath(FileUpload1.FileName);
                Param.MaxGroupNo = Int32.Parse(TextBox1.Text);
                FileUpload1.SaveAs(Param.FilePath);
                Param.Delimiter = ",";
                RepeatReadCsvToArray(ref Param);

                StandardKmeans(ref Param);

                CreateGridView(Param);
                
            }
            else
            {
                this.Label2.Text = "<font color='red'>請選擇資料檔</font>";
            }
        }

        private void CreateGridView(KmeansParam Param)
        {

            GridView1.DataSource = Param.DetailsTable;
            GridView1.DataBind();

            GridView2.DataSource = Param.SumTable;
            GridView2.DataBind();

            GridView3.DataSource = Param.InitVectorTable;
            GridView3.DataBind();
        }
        
        #region -- Compute Euclidean distance (allow any dimenstaion) --

        protected double DistanceSquared(double[] p, double[] q)
        {
            int d = p.Length; // dimension of vectors
            double sum = 0;
            if (d != q.Length)
            {
                throw new Exception("p and q vectors must be the same length");
            }

            for (int i = 0; i < d; i++)
            {
                sum += Math.Pow((p[i] - q[i]), 2);
            }
            return sum;
        }

        protected double Distance(double[] p, double[] q)
        {
            return Math.Sqrt(DistanceSquared(p, q));
        }
        #endregion

        
        #region -- ready CSV data to memory --
        protected void RepeatReadCsvToArray(ref KmeansParam Param)
        {
            StreamReader s = new StreamReader(Param.FilePath, System.Text.Encoding.Default);
            s.ReadLine();//skip  first line

            string AllData = s.ReadToEnd().Trim();
            string delimiter = Param.Delimiter;
            string[] rows = AllData.Split("\n".ToCharArray());
            string[] cols = rows[0].Split(delimiter.ToCharArray());
            int records = Param.NumOfRows = rows.Length;
            int fields = Param.NumOfCols = cols.Length;

            double[][] table = new double[records][];

            for (int k = 0; k < records; k++)
            {
                string[] items = rows[k].Trim().Split(delimiter.ToCharArray());
                table[k] = new double[fields];
                table[k] = Array.ConvertAll(items, (v) => { return Convert.ToDouble(v); });
            }

            s.Close();

            Param.Record = table;
        }
        
        #endregion
        
        protected void RepeatAssignToGroup(int GroupNo, ref KmeansParam Param)
        {
            int records = Param.NumOfRows;
            int[] cluster = new int[records];
            //double distortion = 0;
            double[][] DataRecord = Param.Record;
            double[][] centroid = Param.CurrentCentroid;
            Array.Clear(cluster, 0, cluster.Length);

            // calculate the distance between center point and coordinate .
            for (int i = 0; i < records; i++)
            {
                double min = 100000;
                for (int j = 0; j < GroupNo; j++)
                {
                    double dist = Distance(DataRecord[i], centroid[j]);
                    if (dist < min)
                    {
                        min = dist;
                        // assign data[i] to group[j]
                        cluster[i] = j;
                    }
                }
                //distortion += min;
            }

            //Param.Distortion = distortion;
            Param.BelongToCluster = cluster;
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            KmeansParam Param = new KmeansParam();
            if (FileUpload1.HasFile)
            {
                // 建立資料夾
                string realPath = GetUploadFolderPath();
                if (!Directory.Exists(realPath))
                {
                    Directory.CreateDirectory(realPath);
                }

                Param.FilePath = GetFilePath(FileUpload1.FileName);
                Param.MaxGroupNo = Int32.Parse(TextBox2.Text);
                Param.Delimiter = ",";

                FileUpload1.SaveAs(Param.FilePath);

                DateTime d1 = DateTime.Now;
                RepeatReadCsvToArray(ref Param);
                RepeatKmeans(ref Param);
                GetDistortionData(ref Param);
                CreateChart(Param);

                DateTime d2 = DateTime.Now;
                TimeSpan ts = d2 - d1;

                this.Label1.Text = "<font color='red'>最適分群數: " + Param.BestK + "</font>";
                this.Label1.Text += "<br/>Elapse time: " + ts;
            }
            else
            {
                this.Label2.Text = "<font color='red'>請選擇資料檔</font>";
            }
        }

        #region -- Get candidate centroid for K-Means++ --
        protected int RepeatGetNextCandidate(double[] delta)
        {

            Random random = new Random((int)DateTime.Now.Millisecond);
            double rand;
            int i;
            double sum = 0;
            int records = delta.Length;

            //Array.ConvertAll(delta, (v) => { return Math.Pow(v, 2); }); 

            for (i = 0; i < records; i++)
            {
                sum += delta[i];
            }

            rand = random.NextDouble() * sum;
            for (i = 0; i < records; i++)
            {
                rand -= delta[i];
                if (rand <= 0)
                {
                    break;
                }
            }
            return i;
        }
#if false
        protected void AssignRecord(int len, double[] t, double[] s)
        {
            t = new double[len];
            for (int i = 0; i < len; i++)
            {
                t[i] = s[i];
            }
        }
#endif
        protected int GetRandRecordIndex(int range)
        {
            Random random = new Random((int)DateTime.Now.Millisecond);
            return random.Next(0, range);
        } 
        #endregion

        #region -- Preprocessing (K-Means++) --
        protected void PreProcessing(int GroupNo, ref KmeansParam Param)
        {
            int fields = Param.NumOfCols;
            int records = Param.NumOfRows;
            double[][] record = Param.Record;

            int Candidate;
            double[][] centroid = new double[GroupNo][];
            double[] delta = new double[records];
#if true
            for (int i = 0; i < records; i++)
            {
                delta[i] = 1000000;
            }
#else
            Array.ConvertAll(delta, (v) => { return 1000000; });
#endif
            for (int k = 0; k < GroupNo; k++)
            {
                // pick a candidate for centroid
                if (k == 0)
                {
                    Candidate = GetRandRecordIndex(records);
                }
                else
                {
                    Candidate = RepeatGetNextCandidate(delta);
                }

                // create a new centroid
                centroid[k] = new double[fields];
                Array.Copy(record[Candidate], centroid[k], fields); 
                
                for (int i = 0; i < records; i++)
                {
#if false
                    double min = 100000;
                    for (int j = 0; j <= k; j++)
                    {
                        double mindDistance;
                        // need to improve this
                        mindDistance = Distance(record[i], centroid[j]);
                        if (mindDistance < min)
                        {
                            min = mindDistance;
                            delta[i] = Math.Pow(mindDistance, 2);
                        }
                    }
#else
                    double min = Math.Pow(Distance(record[i], centroid[k]), 2);
                    if (min < delta[i])
                    {
                        delta[i] = min;
                    }
#endif
                }
            }
#if false
            // patch. 
            Param.InitCentroid = centroid;
            for(int i = 0; i < GroupNo; i++)
            {
                Param.CurrentCentroid[i] = new double[fields];
                Array.Copy(centroid[i], Param.CurrentCentroid[i], fields);
            }
#else
            Param.InitCentroid = Param.CurrentCentroid = centroid;
#endif
            
        } 
        #endregion

        #region -- K-Means --
        protected void RepeatKmeans(ref KmeansParam Param)
        {
            int records = Param.NumOfRows;
            int fields = Param.NumOfCols;
            int start = Param.GroupNoStart = 2;
            int repeat = Param.RepeatTimes = 100;
            int MaxGroupNo = Param.MaxGroupNo;
            int len = MaxGroupNo - start + 1;
            Param.Distortion = new double[len][];
            Param.GroupNo = new int[len];

            for (int i = start; i <= MaxGroupNo; i++)
            {
                Param.GroupNo[i - start] = i;
                Param.Distortion[i - start] = new double[repeat];
                for (int j = 0; j < repeat; j++)
                {
                    PreProcessing(i, ref Param);
                    do
                    {
                        RepeatAssignToGroup(i, ref Param);
                        UpdateCentroid(i, ref Param);
                    } while (!RepeatStabled(i, ref Param));

                    GetRepeatDistortion(i, j, ref Param);
                }
            }

        } 
        #endregion

        #region -- Chart (repeat mode) --
        private void CreateChart(KmeansParam Param)
        {

            int[] xValues = Param.GroupNo;
            string[] titleArr = { "Max. SSE", "Avg. SSE", "Min. SSE" };
            double[] yValues2 = Param.AvgDistortion;
            double[] yValues = Param.MaxDistortion;
            double[] yValues3 = Param.MinDistortion;

            //ChartAreas,Series,Legends 基本設定
            Chart Chart1 = new Chart();
            Chart1.ChartAreas.Add("ChartArea1"); //圖表區域集合
            Chart1.Series.Add("Series1"); //數據序列集合
            Chart1.Series.Add("Series2");
            Chart1.Series.Add("Series3");
            Chart1.Legends.Add("Legends1"); //圖例集合

            //設定 Chart
            Chart1.Width = 800;
            Chart1.Height = 500;
            Title title = new Title();
            title.Text = "K-Means++";
            title.Alignment = ContentAlignment.MiddleCenter;
            title.Font = new System.Drawing.Font("Trebuchet MS", 14F, FontStyle.Bold);
            Chart1.Titles.Add(title);

            //設定 ChartArea
            //Chart1.ChartAreas["ChartArea1"].Area3DStyle.Enable3D = false; //3D效果
            //Chart1.ChartAreas["ChartArea1"].Area3DStyle.IsClustered = false; //並排顯示
            //Chart1.ChartAreas["ChartArea1"].Area3DStyle.Rotation = 40; //垂直角度
            //Chart1.ChartAreas["ChartArea1"].Area3DStyle.Inclination = 50; //水平角度
            //Chart1.ChartAreas["ChartArea1"].Area3DStyle.PointDepth = 30; //數據條深度
            //Chart1.ChartAreas["ChartArea1"].Area3DStyle.WallWidth = 0; //外牆寬度
            ////Chart1.ChartAreas["ChartArea1"].Area3DStyle.LightStyle = LightStyle.Realistic; //光源
            ////Chart1.ChartAreas["ChartArea1"].BackColor = Color.FromArgb(240, 240, 240); //背景色
            //Chart1.ChartAreas["ChartArea1"].AxisX2.Enabled = AxisEnabled.Auto; //隱藏 X2 標示
            //Chart1.ChartAreas["ChartArea1"].AxisY2.Enabled = AxisEnabled.Auto; //隱藏 Y2 標示
            //Chart1.ChartAreas["ChartArea1"].AxisY2.MajorGrid.Enabled = false;   //隱藏 Y2 軸線
            ////Y 軸線顏色
            Chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.LineColor = Color.FromArgb(150, 150, 150);
            Chart1.ChartAreas["ChartArea1"].AxisY.Title = "SSE (Sample 100 times)";
            //X 軸線顏色
            Chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.LineColor = Color.FromArgb(150, 150, 150);
            Chart1.ChartAreas["ChartArea1"].AxisX.Title = "Number of Group";
            //Chart1.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "#,###";
            //Chart1.ChartAreas["ChartArea1"].AxisY2.Maximum = 160;
            //Chart1.ChartAreas["ChartArea1"].AxisY2.Interval = 20;

            //設定 Legends            
            Chart1.Legends["Legends1"].DockedToChartArea = "ChartArea1"; //顯示在圖表內
            //Chart1.Legends["Legends1"].Docking = Docking.Bottom; //自訂顯示位置
            //Chart1.Legends["Legends1"].BackColor = Color.FromArgb(235, 235, 235); //背景色
            //斜線背景
            Chart1.Legends["Legends1"].BackHatchStyle = ChartHatchStyle.DarkDownwardDiagonal;
            Chart1.Legends["Legends1"].BorderWidth = 1;
            Chart1.Legends["Legends1"].BorderColor = Color.FromArgb(200, 200, 200);

            //設定 Series
            Chart1.Series["Series1"].ChartType = SeriesChartType.Spline; //直條圖
            //Chart1.Series["Series1"].ChartType = SeriesChartType.Bar; //橫條圖
            Chart1.Series["Series1"].Points.DataBindXY(xValues, yValues);
            Chart1.Series["Series1"].Legend = "Legends1";
            Chart1.Series["Series1"].LegendText = titleArr[0];
            Chart1.Series["Series1"].LabelFormat = "#,###"; //金錢格式
            Chart1.Series["Series1"].MarkerSize = 8; //Label 範圍大小
            Chart1.Series["Series1"].LabelForeColor = Color.FromArgb(0, 90, 255); //字體顏色
            //字體設定
            Chart1.Series["Series1"].Font = new System.Drawing.Font("Trebuchet MS", 10, System.Drawing.FontStyle.Bold);
            //Label 背景色
            Chart1.Series["Series1"].LabelBackColor = Color.FromArgb(150, 255, 255, 255);
            Chart1.Series["Series1"].Color = Color.FromArgb(240, 65, 140, 240); //背景色
            Chart1.Series["Series1"].IsValueShownAsLabel = false; // Show data points labels

            Chart1.Series["Series2"].ChartType = SeriesChartType.Spline; //直條圖
            Chart1.Series["Series2"].Points.DataBindXY(xValues, yValues2);
            Chart1.Series["Series2"].Legend = "Legends1";
            Chart1.Series["Series2"].LegendText = titleArr[1];
            Chart1.Series["Series2"].LabelFormat = "#,###"; //金錢格式
            Chart1.Series["Series2"].MarkerSize = 8; //Label 範圍大小
            Chart1.Series["Series2"].LabelForeColor = Color.FromArgb(255, 103, 0);
            Chart1.Series["Series2"].Font = new System.Drawing.Font("Trebuchet MS", 10, FontStyle.Bold);
            Chart1.Series["Series2"].LabelBackColor = Color.FromArgb(150, 255, 255, 255);
            Chart1.Series["Series2"].Color = Color.FromArgb(240, 252, 180, 65); //背景色
            if (Param.MaxGroupNo < 50)
            {
                Chart1.Series["Series2"].MarkerStyle = MarkerStyle.Circle;
                Chart1.Series["Series2"].IsValueShownAsLabel = true; //顯示數據
            }
            else
            {
                Chart1.Series["Series2"].IsValueShownAsLabel = false; //顯示數據
            }
            Chart1.Series["Series3"].ChartType = SeriesChartType.Spline; //直條圖
            Chart1.Series["Series3"].Points.DataBindXY(xValues, yValues3);
            Chart1.Series["Series3"].Legend = "Legends1";
            Chart1.Series["Series3"].LegendText = titleArr[2];
            Chart1.Series["Series3"].LabelFormat = "#,###"; //金錢格式
            Chart1.Series["Series3"].MarkerSize = 8; //Label 範圍大小
            Chart1.Series["Series3"].LabelForeColor = Color.FromArgb(0, 255, 0);
            Chart1.Series["Series3"].Font = new System.Drawing.Font("Trebuchet MS", 10, FontStyle.Bold);
            Chart1.Series["Series3"].LabelBackColor = Color.FromArgb(150, 255, 255, 255);
            Chart1.Series["Series3"].Color = Color.FromArgb(240, 0, 220, 0); //背景色
            Chart1.Series["Series3"].IsValueShownAsLabel = false; //顯示數據

            Page.Controls.Add(Chart1);
        } 
        #endregion

        private void GetDistortionData(ref KmeansParam Param)
        {
            int repeat = Param.RepeatTimes;
            int start = Param.GroupNoStart;
            int len = Param.MaxGroupNo-Param.GroupNoStart+1;
            double[] MinDist = new double[len];
            double[] MaxDist = new double[len];
            double[] AvgDist = new double[len];
            double[][] distortion = Param.Distortion;
            

            for (int i = 0; i < len; i++)
            {
                double sum = 0;
                double max = 0;
                double min = 1000000;

                for (int j = 0; j < repeat; j++)
                {
                    if (distortion[i][j] < min)
                    {
                        min = distortion[i][j];
                    }
                    if (distortion[i][j] > max)
                    {
                        max = distortion[i][j];
                    }
                    sum += distortion[i][j];
                }
                MinDist[i] = min;
                MaxDist[i] = max;
                AvgDist[i] = sum / repeat;
            }

            Param.MaxDistortion = MaxDist;
            Param.MinDistortion = MinDist;
            Param.AvgDistortion = AvgDist;

            GetSuggestK(ref Param);
        }

        private void GetSuggestK(ref KmeansParam Param)
        {
            double[] dist = Param.AvgDistortion;
            int len = Param.AvgDistortion.Length;
            double max = 0;
            for(int i = 1; i < len; i++)
            {
                double descent = dist[i-1] - dist[i];
                if (descent > max)
                {
                    max = descent;
                    Param.BestK = i + Param.GroupNoStart;
                }
            }
            
        }

        private void GetRepeatDistortion(int GroupNo, int repeat, ref KmeansParam Param)
        {
            int records = Param.NumOfRows;
            int[] cluster = Param.BelongToCluster;
            double[][] DataRecord = Param.Record;
            double[][] centroid = Param.CurrentCentroid;
            double distortion = 0;
            int start = Param.GroupNoStart;
 
            for (int i = 0; i < records; i++)
            {
                for (int j = 0; j < GroupNo; j++)
                {
                    if(cluster[i] == j)
                    {
                        distortion += Distance(DataRecord[i], centroid[j]);
                    }
                }
            }
            Param.Distortion[GroupNo-start][repeat] = distortion;
        }

        protected bool RepeatStabled(int GroupNo, ref KmeansParam Param)
        {
            int fields = Param.NumOfCols;
            double[][] updated = Param.UpdatedCentroid;
            double[][] current = Param.CurrentCentroid;

            for (int i = 0; i < GroupNo; i++)
            {
                for (int j = 0; j < fields; j++)
                {
                    if (Math.Round(updated[i][j], 3) != Math.Round(current[i][j], 3))
                    {
                        for (int k = 0; k < GroupNo; k++)
                        {
#if false
                            Array.Copy(updated[k], current[k], fields); 
#else
                            Param.CurrentCentroid = Param.UpdatedCentroid;
#endif
                        }
                        return false;
                    }
                }
            }

            return true;
        }

        protected void UpdateCentroid(int GroupNo, ref KmeansParam Param)
        {
            int DataDimension = Param.NumOfCols;
            double[] SumOfCoordinate = new double[DataDimension];
            int NoOfCoordinate;
            int records = Param.NumOfRows;
            int[] cluster = Param.BelongToCluster;
            double[][] NewCentroid = new double[GroupNo][];
            double[][] DataRecord = Param.Record;

            for (int j = 0; j < GroupNo; j++)
            {
#if false
                for (int k = 0; k < DataDimension; k++)
                {
                    SumOfCoordinate[k] = 0;
                }
#else
                Array.Clear(SumOfCoordinate, 0, SumOfCoordinate.Length);
#endif
                NoOfCoordinate = 0;
                for (int i = 0; i < records; i++)
                {
                    int DataGroupNo = cluster[i];

                    if (DataGroupNo == j)
                    {
                        //  summation of the same group coordinate
                        for (int k = 0; k < DataDimension; k++)
                        {
                            SumOfCoordinate[k] += DataRecord[i][k];
                        }
                        
                        NoOfCoordinate++;
                    }
                }

                NewCentroid[j] = new double[DataDimension];
                
                if (NoOfCoordinate != 0)
                {
                    for (int i = 0; i < DataDimension; i++)
                    {
                        NewCentroid[j][i] = Math.Round(SumOfCoordinate[i] / NoOfCoordinate, 3);
                    }
                }
                else
                {
                    Array.Copy(Param.CurrentCentroid[j], NewCentroid[j], DataDimension);
                    //Response.Write("<h1>zero group: </h1>");
                    //ShowMessage("zero group：");
                }
            }
            Param.UpdatedCentroid = NewCentroid;
        }
        #region -- for debug only --

        void ShowMessage(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Response.Write(message + " at line " + lineNumber + " (" + caller + ")");
        } 
        #endregion

    }
}

