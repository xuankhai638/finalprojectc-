using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace EnglishLearningApp
{
    public partial class MainForm : Form
    {
        private string connectionString = "Server=LAPTOP-09GEG9I7;Database=EnglishLearningAppDB;Integrated Security=True;TrustServerCertificate=True;";
        private int currentUserId = 0;
        private string selectedLanguagePair = "Vietnamese-English";

        // UI controls
        private Panel gamePanel, libraryPanel;
        private Button btnGames, btnLibrary, btnLogout, btnBack;
        private Label lblUsername, lblPassword;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnShowRegister;
        private Label lblRegUsername, lblRegPassword, lblRegFullName, lblRegEmail;
        private TextBox txtRegUsername, txtRegPassword, txtRegFullName, txtRegEmail;
        private Button btnRegister, btnBackToLogin;
        private Label lblTitleLanguageSelection;
        private ComboBox cmbLanguagePair;
        private Button btnConfirmLanguageSelection;

        // Data for questions and vocabulary
        private Dictionary<string, List<Question>> multipleChoiceTopics;
        private List<TrueFalseQuestion> trueFalseQuestions;
        private List<FillInBlankQuestion> fillInBlankQuestions;
        private List<GrammarQuestion> grammarQuestions;
        private List<VocabularyWord> vocabularyWords;
        private int currentQuestionIndex = 0;
        private int score = 0;
        private string currentQuizType = "";
        private string selectedTopic = "";
        private string navigationState = "MainMenu";

        // Controls for quizzes
        private Label lblQuestion, lblScore;
        private Button btnOptionA, btnOptionB, btnOptionC, btnOptionD;
        private Button btnTrue, btnFalse;
        private TextBox txtFillInBlankAnswer;
        private Button btnSubmitFillInBlank;
        private RadioButton rdoGrammarOption1, rdoGrammarOption2, rdoGrammarOption3, rdoGrammarOption4;
        private Button btnSubmitGrammar;
        private PictureBox pictureBox;
        private Panel explanationPanel;
        private bool isImageVisible = true;

        // Data classes
        private class VocabularyWord
        {
            public string Word { get; set; }
            public string Pronunciation { get; set; }
            public string Meaning { get; set; }
            public string Explanation { get; set; }
            public string Example { get; set; }
            public string NativeWord { get; set; }
            public string NativeMeaning { get; set; }
            public string NativeExplanation { get; set; }
            public string NativeExample { get; set; }
            public VocabularyWord(string word, string pronunciation, string meaning, string explanation, string example,
                                 string nativeWord, string nativeMeaning, string nativeExplanation, string nativeExample)
            {
                Word = word; Pronunciation = pronunciation; Meaning = meaning; Explanation = explanation; Example = example;
                NativeWord = nativeWord; NativeMeaning = nativeMeaning; NativeExplanation = nativeExplanation; NativeExample = nativeExample;
            }
        }

        private class Question
        {
            public string Text { get; set; }
            public string NativeText { get; set; }
            public string[] Options { get; set; }
            public string CorrectAnswer { get; set; }
            public string ImagePath { get; set; }
            public string ImageExplanation { get; set; }
            public string NativeImageExplanation { get; set; }
            public Question(string text, string nativeText, string[] options, string correctAnswer, string imagePath = "", string imageExplanation = "", string nativeImageExplanation = "")
            {
                Text = text; NativeText = nativeText; Options = options; CorrectAnswer = correctAnswer; ImagePath = imagePath; ImageExplanation = imageExplanation; NativeImageExplanation = nativeImageExplanation;
            }
        }

        private class TrueFalseQuestion
        {
            public string Text { get; set; }
            public string NativeText { get; set; }
            public bool IsTrue { get; set; }
            public TrueFalseQuestion(string text, string nativeText, bool isTrue)
            {
                Text = text; NativeText = nativeText; IsTrue = isTrue;
            }
        }

        private class FillInBlankQuestion
        {
            public string Text { get; set; }
            public string NativeText { get; set; }
            public string CorrectAnswer { get; set; }
            public string Hint { get; set; }
            public string NativeHint { get; set; }
            public FillInBlankQuestion(string text, string nativeText, string correctAnswer, string hint, string nativeHint)
            {
                Text = text; NativeText = nativeText; CorrectAnswer = correctAnswer; Hint = hint; NativeHint = nativeHint;
            }
        }

        private class GrammarQuestion
        {
            public string Text { get; set; }
            public string NativeText { get; set; }
            public string[] Options { get; set; }
            public string CorrectAnswer { get; set; }
            public string Explanation { get; set; }
            public string NativeExplanation { get; set; }
            public GrammarQuestion(string text, string nativeText, string[] options, string correctAnswer, string explanation, string nativeExplanation)
            {
                Text = text; NativeText = nativeText; Options = options; CorrectAnswer = correctAnswer; Explanation = explanation; NativeExplanation = nativeExplanation;
            }
        }

        public MainForm()
        {
            // Xóa InitializeComponent() vì không dùng Designer
            InitializeLoginUI();
            InitializeMainUI();
            InitializeData();
        }

        private void InitializeLoginUI()
        {
            this.Text = "English Learning App";
            this.Size = new Size(800, 700);
            this.BackColor = Color.FromArgb(255, 245, 224);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblUsername = new Label { Text = "Username:", Location = new Point(150, 100), Size = new Size(100, 30), Font = new Font("Arial", 12) };
            txtUsername = new TextBox { Location = new Point(250, 100), Size = new Size(200, 30), Font = new Font("Arial", 12) };
            lblPassword = new Label { Text = "Password:", Location = new Point(150, 150), Size = new Size(100, 30), Font = new Font("Arial", 12) };
            txtPassword = new TextBox { Location = new Point(250, 150), Size = new Size(200, 30), Font = new Font("Arial", 12), UseSystemPasswordChar = true };
            btnLogin = new Button { Text = "Login", Location = new Point(250, 200), Size = new Size(100, 40), BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnShowRegister = new Button { Text = "Register", Location = new Point(350, 200), Size = new Size(100, 40), BackColor = Color.Orange, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };

            btnLogin.Click += BtnLogin_Click;
            btnShowRegister.Click += BtnShowRegister_Click;

            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnShowRegister });
        }

        private void InitializeMainUI()
        {
            gamePanel = new Panel { Location = new Point(10, 60), Size = new Size(800, 700), BackColor = Color.LightCyan, Visible = false };
            libraryPanel = new Panel { Location = new Point(10, 60), Size = new Size(800, 700), BackColor = Color.LightCyan, Visible = false };
            btnGames = new Button { Text = "Games", Location = new Point(150, 10), Size = new Size(120, 40), BackColor = Color.Orange, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), Visible = false };
            btnLibrary = new Button { Text = "Library", Location = new Point(280, 10), Size = new Size(120, 40), BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), Visible = false };
            btnLogout = new Button { Text = "Logout", Location = new Point(410, 10), Size = new Size(120, 40), BackColor = Color.Red, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), Visible = false };
            btnBack = new Button { Text = "Back", Location = new Point(10, 10), Size = new Size(100, 40), BackColor = Color.Pink, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), Visible = false };

            btnGames.Click += BtnGames_Click;
            btnLibrary.Click += BtnLibrary_Click;
            btnLogout.Click += (s, e) => { HideMainUI(); ShowLoginUI(); currentUserId = 0; txtUsername.Clear(); txtPassword.Clear(); navigationState = "MainMenu"; };
            btnBack.Click += BtnBack_Click;

            if (!this.Controls.Contains(gamePanel)) this.Controls.Add(gamePanel);
            if (!this.Controls.Contains(libraryPanel)) this.Controls.Add(libraryPanel);
            if (!this.Controls.Contains(btnGames)) this.Controls.Add(btnGames);
            if (!this.Controls.Contains(btnLibrary)) this.Controls.Add(btnLibrary);
            if (!this.Controls.Contains(btnLogout)) this.Controls.Add(btnLogout);
            if (!this.Controls.Contains(btnBack)) this.Controls.Add(btnBack);
        }

        private void InitializeData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Load VocabularyWords
                    vocabularyWords = new List<VocabularyWord>();
                    string vocabQuery = "SELECT * FROM VocabularyWords";
                    using (SqlCommand cmd = new SqlCommand(vocabQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vocabularyWords.Add(new VocabularyWord(
                                    reader["Word"].ToString(),
                                    reader["Pronunciation"].ToString(),
                                    reader["Meaning"].ToString(),
                                    reader["Explanation"].ToString(),
                                    reader["Example"].ToString(),
                                    reader["NativeWord"].ToString(),
                                    reader["NativeMeaning"].ToString(),
                                    reader["NativeExplanation"].ToString(),
                                    reader["NativeExample"].ToString()
                                ));
                            }
                        }
                    }

                    // Load MultipleChoiceQuestions
                    multipleChoiceTopics = new Dictionary<string, List<Question>>();
                    string mcQuery = "SELECT * FROM MultipleChoiceQuestions";
                    using (SqlCommand cmd = new SqlCommand(mcQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string topic = reader["Topic"].ToString();
                                if (!multipleChoiceTopics.ContainsKey(topic))
                                {
                                    multipleChoiceTopics[topic] = new List<Question>();
                                }
                                multipleChoiceTopics[topic].Add(new Question(
                                    reader["Text"].ToString(),
                                    reader["NativeText"].ToString(),
                                    new[] { reader["OptionA"].ToString(), reader["OptionB"].ToString(), reader["OptionC"].ToString(), reader["OptionD"].ToString() },
                                    reader["CorrectAnswer"].ToString(),
                                    reader["ImagePath"].ToString(),
                                    reader["ImageExplanation"].ToString(),
                                    reader["NativeImageExplanation"].ToString()
                                ));
                            }
                        }
                    }

                    // Load TrueFalseQuestions
                    trueFalseQuestions = new List<TrueFalseQuestion>();
                    string tfQuery = "SELECT * FROM TrueFalseQuestions";
                    using (SqlCommand cmd = new SqlCommand(tfQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                trueFalseQuestions.Add(new TrueFalseQuestion(
                                    reader["Text"].ToString(),
                                    reader["NativeText"].ToString(),
                                    (bool)reader["IsTrue"]
                                ));
                            }
                        }
                    }

                    // Load FillInBlankQuestions
                    fillInBlankQuestions = new List<FillInBlankQuestion>();
                    string fibQuery = "SELECT * FROM FillInBlankQuestions";
                    using (SqlCommand cmd = new SqlCommand(fibQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fillInBlankQuestions.Add(new FillInBlankQuestion(
                                    reader["Text"].ToString(),
                                    reader["NativeText"].ToString(),
                                    reader["CorrectAnswer"].ToString(),
                                    reader["Hint"].ToString(),
                                    reader["NativeHint"].ToString()
                                ));
                            }
                        }
                    }

                    // Load GrammarQuestions
                    grammarQuestions = new List<GrammarQuestion>();
                    string grammarQuery = "SELECT * FROM GrammarQuestions";
                    using (SqlCommand cmd = new SqlCommand(grammarQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                grammarQuestions.Add(new GrammarQuestion(
                                    reader["Text"].ToString(),
                                    reader["NativeText"].ToString(),
                                    new[] { reader["Option1"].ToString(), reader["Option2"].ToString(), reader["Option3"].ToString(), reader["Option4"].ToString() },
                                    reader["CorrectAnswer"].ToString(),
                                    reader["Explanation"].ToString(),
                                    reader["NativeExplanation"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu từ cơ sở dữ liệu: {ex.Message}", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeRegisterUI()
        {
            HideLoginUI();
            lblRegUsername = new Label { Text = "Username:", Location = new Point(150, 50), Size = new Size(100, 30), Font = new Font("Arial", 12) };
            txtRegUsername = new TextBox { Location = new Point(250, 50), Size = new Size(200, 30), Font = new Font("Arial", 12) };
            lblRegPassword = new Label { Text = "Password:", Location = new Point(150, 100), Size = new Size(100, 30), Font = new Font("Arial", 12) };
            txtRegPassword = new TextBox { Location = new Point(250, 100), Size = new Size(200, 30), Font = new Font("Arial", 12), UseSystemPasswordChar = true };
            lblRegFullName = new Label { Text = "Full Name:", Location = new Point(150, 150), Size = new Size(100, 30), Font = new Font("Arial", 12) };
            txtRegFullName = new TextBox { Location = new Point(250, 150), Size = new Size(200, 30), Font = new Font("Arial", 12) };
            lblRegEmail = new Label { Text = "Email:", Location = new Point(150, 200), Size = new Size(100, 30), Font = new Font("Arial", 12) };
            txtRegEmail = new TextBox { Location = new Point(250, 200), Size = new Size(200, 30), Font = new Font("Arial", 12) };
            btnRegister = new Button { Text = "Register", Location = new Point(250, 250), Size = new Size(100, 40), BackColor = Color.Green, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnBackToLogin = new Button { Text = "Back", Location = new Point(350, 250), Size = new Size(100, 40), BackColor = Color.Pink, ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold) };

            btnRegister.Click += BtnRegister_Click;
            btnBackToLogin.Click += BtnBackToLogin_Click;

            this.Controls.AddRange(new Control[] { lblRegUsername, txtRegUsername, lblRegPassword, txtRegPassword, lblRegFullName, txtRegFullName, lblRegEmail, txtRegEmail, btnRegister, btnBackToLogin });
        }

        private void HideLoginUI() => new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnShowRegister }.ToList().ForEach(c => c.Visible = false);
        private void ShowLoginUI() => new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnShowRegister }.ToList().ForEach(c => c.Visible = true);
        private void HideMainUI()
        {
            btnGames.Visible = false;
            btnLibrary.Visible = false;
            btnLogout.Visible = false;
            btnBack.Visible = false;
            gamePanel.Visible = false;
            libraryPanel.Visible = false;
        }
        private void ShowMainUI()
        {
            if (!this.Controls.Contains(btnGames)) this.Controls.Add(btnGames);
            if (!this.Controls.Contains(btnLibrary)) this.Controls.Add(btnLibrary);
            if (!this.Controls.Contains(btnLogout)) this.Controls.Add(btnLogout);
            if (!this.Controls.Contains(gamePanel)) this.Controls.Add(gamePanel);
            if (!this.Controls.Contains(libraryPanel)) this.Controls.Add(libraryPanel);
            if (!this.Controls.Contains(btnBack)) this.Controls.Add(btnBack);

            btnGames.Visible = true;
            btnLibrary.Visible = true;
            btnLogout.Visible = true;
            btnBack.Visible = false;
            gamePanel.Visible = false;
            libraryPanel.Visible = false;
        }
        private void HideRegisterUI() => new Control[] { lblRegUsername, txtRegUsername, lblRegPassword, txtRegPassword, lblRegFullName, txtRegFullName, lblRegEmail, txtRegEmail, btnRegister, btnBackToLogin }.ToList().ForEach(c => c.Visible = false);

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập cả tên người dùng và mật khẩu. / Please enter both username and password.", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, FullName FROM Users WHERE Username = @Username AND Password = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentUserId = reader.GetInt32(0);
                                string fullName = reader.GetString(1);
                                MessageBox.Show($"Chào mừng, {fullName}! Hãy chọn ngôn ngữ học tập. / Welcome, {fullName}! Let's select your learning language.", "Thành công / Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                HideLoginUI();
                                ShowLanguageSelection();
                                navigationState = "LanguageSelection";
                            }
                            else
                            {
                                MessageBox.Show("Tên người dùng hoặc mật khẩu không đúng. / Invalid username or password.", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message} / Login error: {ex.Message}", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowLanguageSelection()
        {
            this.Controls.Clear();

            lblTitleLanguageSelection = new Label
            {
                Text = "Chọn cặp ngôn ngữ / Select Language Pair",
                Location = new Point(0, 20),
                Size = new Size(600, 40),
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            cmbLanguagePair = new ComboBox
            {
                Location = new Point(200, 100),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12)
            };
            cmbLanguagePair.Items.AddRange(new string[] { "Vietnamese-English", "English-English" });
            cmbLanguagePair.SelectedIndex = 0;

            btnConfirmLanguageSelection = new Button
            {
                Text = "Confirm",
                Location = new Point(250, 150),
                Size = new Size(100, 40),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            btnConfirmLanguageSelection.Click += (s, e) =>
            {
                selectedLanguagePair = cmbLanguagePair.SelectedItem.ToString();
                HideLanguageSelection();
                ShowMainUI();
                navigationState = "MainMenu";
            };

            this.Controls.AddRange(new Control[] { lblTitleLanguageSelection, cmbLanguagePair, btnConfirmLanguageSelection });
        }

        private void HideLanguageSelection()
        {
            lblTitleLanguageSelection.Visible = false;
            cmbLanguagePair.Visible = false;
            btnConfirmLanguageSelection.Visible = false;
        }

        private void BtnShowRegister_Click(object sender, EventArgs e) => InitializeRegisterUI();

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtRegUsername.Text.Trim();
            string password = txtRegPassword.Text.Trim();
            string fullName = txtRegFullName.Text.Trim();
            string email = txtRegEmail.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường. / Please fill in all fields.", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ email hợp lệ. / Please enter a valid email address.", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        int userCount = (int)checkCmd.ExecuteScalar();
                        if (userCount > 0)
                        {
                            MessageBox.Show("Tên người dùng đã tồn tại. / Username already exists.", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO Users (Username, Password, FullName, Email) VALUES (@Username, @Password, @FullName, @Email)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập. / Registration successful! Please log in.", "Thành công / Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    HideRegisterUI();
                    ShowLoginUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng ký: {ex.Message} / Registration error: {ex.Message}", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void BtnBackToLogin_Click(object sender, EventArgs e)
        {
            HideRegisterUI();
            ShowLoginUI();
        }

        private void BtnGames_Click(object sender, EventArgs e)
        {
            gamePanel.Visible = true;
            libraryPanel.Visible = false;
            btnBack.Visible = true;
            btnGames.Visible = false;
            btnLibrary.Visible = false;
            btnLogout.Visible = false;
            navigationState = "GameSelection";
            ShowGameSelection();
        }

        private void BtnLibrary_Click(object sender, EventArgs e)
        {
            gamePanel.Visible = false;
            libraryPanel.Visible = true;
            btnBack.Visible = true;
            btnGames.Visible = false;
            btnLibrary.Visible = false;
            btnLogout.Visible = false;
            navigationState = "Library";
            ShowLibraryOptions();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (navigationState == "LanguageSelection")
            {
                HideLanguageSelection();
                ShowLoginUI();
                navigationState = "MainMenu";
            }
            else if (navigationState == "Quiz")
            {
                if (currentQuizType == "MultipleChoice")
                {
                    gamePanel.Controls.Clear();
                    navigationState = "TopicSelection";
                    ShowMultipleChoiceTopics();
                }
                else
                {
                    gamePanel.Controls.Clear();
                    navigationState = "GameSelection";
                    ShowGameSelection();
                }
                currentQuestionIndex = 0;
                score = 0;
                currentQuizType = "";
                selectedTopic = "";
            }
            else if (navigationState == "TopicSelection")
            {
                gamePanel.Controls.Clear();
                navigationState = "GameSelection";
                ShowGameSelection();
            }
            else if (navigationState == "GameSelection" || navigationState == "Library")
            {
                gamePanel.Visible = false;
                libraryPanel.Visible = false;
                btnBack.Visible = false;
                ShowMainUI();
                navigationState = "MainMenu";
                gamePanel.Controls.Clear();
                libraryPanel.Controls.Clear();
            }
            else if (navigationState == "Vocabulary")
            {
                libraryPanel.Controls.Clear();
                navigationState = "Library";
                ShowLibraryOptions();
            }
        }

        private void ShowGameSelection()
        {
            gamePanel.Controls.Clear();
            Label lblTitle = new Label
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Chọn một trò chơi!" : "Choose a Game!",
                Location = new Point(0, 20),
                Size = new Size(580, 40),
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Button btnMC = new Button { Text = selectedLanguagePair == "Vietnamese-English" ? "Trắc nghiệm" : "Multiple Choice", Location = new Point(290, 100), Size = new Size(220, 60), BackColor = Color.Yellow, Font = new Font("Arial", 12, FontStyle.Bold) };
            Button btnTF = new Button { Text = selectedLanguagePair == "Vietnamese-English" ? "Đúng/Sai" : "True/False", Location = new Point(290, 240), Size = new Size(220, 60), BackColor = Color.Pink, Font = new Font("Arial", 12, FontStyle.Bold) };
            Button btnFIB = new Button { Text = selectedLanguagePair == "Vietnamese-English" ? "Điền vào chỗ trống" : "Fill in the Blank", Location = new Point(290, 380), Size = new Size(220, 60), BackColor = Color.Green, Font = new Font("Arial", 12, FontStyle.Bold) };
            Button btnGrammar = new Button { Text = selectedLanguagePair == "Vietnamese-English" ? "Ngữ pháp" : "Grammar", Location = new Point(290, 520), Size = new Size(220, 60), BackColor = Color.Orange, Font = new Font("Arial", 12, FontStyle.Bold) };

            btnMC.Click += (s, e) => { currentQuizType = "MultipleChoice"; navigationState = "TopicSelection"; ShowMultipleChoiceTopics(); };
            btnTF.Click += (s, e) => { currentQuizType = "TrueFalse"; currentQuestionIndex = 0; score = 0; navigationState = "Quiz"; ShowQuiz(); };
            btnFIB.Click += (s, e) => { currentQuizType = "FillInBlank"; currentQuestionIndex = 0; score = 0; navigationState = "Quiz"; ShowQuiz(); };
            btnGrammar.Click += (s, e) => { currentQuizType = "Grammar"; currentQuestionIndex = 0; score = 0; navigationState = "Quiz"; ShowQuiz(); };

            gamePanel.Controls.AddRange(new Control[] { lblTitle, btnMC, btnTF, btnFIB, btnGrammar });
        }
        private void ShowMultipleChoiceTopics()
        {
            gamePanel.Controls.Clear();
            Label lblTitle = new Label
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Chọn một chủ đề!" : "Choose a Topic!",
                Location = new Point(0, 20),
                Size = new Size(580, 40),
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            List<string> topics = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT Topic FROM MultipleChoiceQuestions";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                topics.Add(reader["Topic"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chủ đề: {ex.Message}", "Lỗi / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int yPosition = 100; // Start at y = 100 for better spacing
            foreach (string topic in topics)
            {
                string displayTopic = selectedLanguagePair == "Vietnamese-English" ?
                    (topic == "Animals" ? "Động vật" :
                     topic == "Transportation" ? "Phương tiện giao thông" :
                     topic == "Body Parts" ? "Bộ phận cơ thể" :
                     topic == "Colors" ? "Màu sắc" : topic) :
                    topic;

                Button btnTopic = new Button
                {
                    Text = displayTopic,
                    Location = new Point(290, yPosition), // Center horizontally
                    Size = new Size(220, 40),
                    BackColor = Color.Yellow,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                btnTopic.Click += (s, e) => { selectedTopic = topic; currentQuestionIndex = 0; score = 0; navigationState = "Quiz"; ShowQuiz(); };
                gamePanel.Controls.Add(btnTopic);
                yPosition += 100; // Increase gap to 60 pixels (40 height + 60 gap)
            }

            gamePanel.Controls.Add(lblTitle);
        }

        private void ShowLibraryOptions()
        {
            libraryPanel.Controls.Clear();
            Label lblTitle = new Label
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Học điều mới!" : "Learn Something New!",
                Location = new Point(0, 20),
                Size = new Size(580, 40),
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Button btnVocab = new Button { Text = selectedLanguagePair == "Vietnamese-English" ? "Từ vựng" : "Vocabulary", Location = new Point(180, 100), Size = new Size(220, 60), BackColor = Color.Yellow, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnVocab.Click += (s, e) => { libraryPanel.Controls.Clear(); navigationState = "Vocabulary"; ShowVocabulary(); };
            libraryPanel.Controls.AddRange(new Control[] { lblTitle, btnVocab });
        }

        private void ShowVocabulary()
        {
            libraryPanel.Controls.Clear();

            Label lblSearch = new Label
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Tìm kiếm từ vựng:" : "Search Vocabulary:",
                Location = new Point(10, 20),
                Size = new Size(150, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            TextBox txtSearch = new TextBox
            {
                Location = new Point(160, 20),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10)
            };

            ListBox lstVocab = new ListBox
            {
                Location = new Point(10, 50),
                Size = new Size(200, 300)
            };

            foreach (var word in vocabularyWords)
            {
                lstVocab.Items.Add(selectedLanguagePair == "Vietnamese-English" ? word.NativeWord : word.Word);
            }

            Label lblWord = new Label { Location = new Point(220, 50), Size = new Size(350, 30), Font = new Font("Arial", 10) };
            Label lblPronunciation = new Label { Location = new Point(220, 90), Size = new Size(350, 30), Font = new Font("Arial", 10) };
            Label lblMeaning = new Label { Location = new Point(220, 130), Size = new Size(350, 30), Font = new Font("Arial", 10) };
            Label lblExplanation = new Label { Location = new Point(220, 170), Size = new Size(350, 60), Font = new Font("Arial", 10) };
            Label lblExample = new Label { Location = new Point(220, 240), Size = new Size(350, 60), Font = new Font("Arial", 10) };
            Label lblNativeWord = new Label { Location = new Point(220, 310), Size = new Size(350, 30), Font = new Font("Arial", 10) };
            Label lblNativeMeaning = new Label { Location = new Point(220, 350), Size = new Size(350, 30), Font = new Font("Arial", 10) };
            Label lblNativeExplanation = new Label { Location = new Point(220, 390), Size = new Size(350, 60), Font = new Font("Arial", 10) };
            Label lblNativeExample = new Label { Location = new Point(220, 460), Size = new Size(350, 60), Font = new Font("Arial", 10) };

            lstVocab.SelectedIndexChanged += (s, e) =>
            {
                if (lstVocab.SelectedItem != null)
                {
                    var word = vocabularyWords.First(w => (selectedLanguagePair == "Vietnamese-English" ? w.NativeWord : w.Word) == lstVocab.SelectedItem.ToString());
                    lblWord.Text = $"Word: {word.Word}";
                    lblPronunciation.Text = $"Pronunciation: {word.Pronunciation}";
                    lblMeaning.Text = $"Meaning: {word.Meaning}";
                    lblExplanation.Text = $"Explanation: {word.Explanation}";
                    lblExample.Text = $"Example: {word.Example}";
                    if (selectedLanguagePair == "Vietnamese-English")
                    {
                        lblNativeWord.Text = "Từ tiếng Việt: " + word.NativeWord;
                        lblNativeMeaning.Text = "Nghĩa tiếng Việt: " + word.NativeMeaning;
                        lblNativeExplanation.Text = "Giải thích tiếng Việt: " + word.NativeExplanation;
                        lblNativeExample.Text = "Ví dụ tiếng Việt: " + word.NativeExample;
                    }
                    else
                    {
                        lblNativeWord.Text = "Word: " + word.Word;
                        lblNativeMeaning.Text = "Meaning: " + word.Meaning;
                        lblNativeExplanation.Text = "Explanation: " + word.Explanation;
                        lblNativeExample.Text = "Example: " + word.Example;
                    }
                }
            };

            txtSearch.TextChanged += (s, e) =>
            {
                string searchText = txtSearch.Text.Trim().ToLower();
                lstVocab.Items.Clear();

                var filteredWords = vocabularyWords
                    .Where(w => (selectedLanguagePair == "Vietnamese-English" ? w.NativeWord : w.Word).ToLower().Contains(searchText))
                    .ToList();

                foreach (var word in filteredWords)
                {
                    lstVocab.Items.Add(selectedLanguagePair == "Vietnamese-English" ? word.NativeWord : word.Word);
                }

                if (lstVocab.Items.Count > 0)
                {
                    lstVocab.SelectedIndex = 0;
                }
                else
                {
                    lblWord.Text = "";
                    lblPronunciation.Text = "";
                    lblMeaning.Text = "";
                    lblExplanation.Text = "";
                    lblExample.Text = "";
                    lblNativeWord.Text = "";
                    lblNativeMeaning.Text = "";
                    lblNativeExplanation.Text = "";
                    lblNativeExample.Text = "";
                }
            };

            if (lstVocab.Items.Count > 0)
            {
                lstVocab.SelectedIndex = 0;
            }

            libraryPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, lstVocab, lblWord, lblPronunciation, lblMeaning, lblExplanation, lblExample, lblNativeWord, lblNativeMeaning, lblNativeExplanation, lblNativeExample });
        }

        private void ShowQuiz()
        {
            gamePanel.Controls.Clear();
            isImageVisible = true;

            if (currentQuizType == "MultipleChoice" && !string.IsNullOrEmpty(selectedTopic) && currentQuestionIndex < multipleChoiceTopics[selectedTopic].Count)
            {
                Question q = multipleChoiceTopics[selectedTopic][currentQuestionIndex];
                lblQuestion = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? q.NativeText : q.Text,
                    Location = new Point(160, 20), // Center horizontally
                    Size = new Size(480, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                Panel cardPanel = new Panel
                {
                    Location = new Point(260, 60), // Center horizontally
                    Size = new Size(280, 150),
                    BorderStyle = BorderStyle.FixedSingle
                };

                pictureBox = new PictureBox
                {
                    Location = new Point(0, 0),
                    Size = new Size(280, 150),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Visible = true
                };

                Label lblImageExplanation = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? q.NativeImageExplanation : q.ImageExplanation,
                    Location = new Point(0, 0),
                    Size = new Size(280, 150),
                    Font = new Font("Arial", 10),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.LightGray,
                    Visible = false
                };

                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string imagePath = Path.Combine(basePath, q.ImagePath);

                if (!string.IsNullOrEmpty(q.ImagePath))
                {
                    try
                    {
                        if (File.Exists(imagePath))
                        {
                            pictureBox.Image = Image.FromFile(imagePath);
                        }
                        else
                        {
                            lblImageExplanation.Text = "Image not found.";
                            lblImageExplanation.Visible = true;
                            pictureBox.Visible = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        lblImageExplanation.Text = $"Error loading image: {ex.Message}";
                        lblImageExplanation.Visible = true;
                        pictureBox.Visible = false;
                    }
                }

                bool isFrontVisible = true;
                cardPanel.Click += (s, e) =>
                {
                    isFrontVisible = !isFrontVisible;
                    pictureBox.Visible = isFrontVisible;
                    lblImageExplanation.Visible = !isFrontVisible;
                };

                cardPanel.Controls.Add(pictureBox);
                cardPanel.Controls.Add(lblImageExplanation);

                btnOptionA = new Button { Text = q.Options[0], Location = new Point(160, 220), Size = new Size(480, 40), BackColor = Color.Yellow, Font = new Font("Arial", 10, FontStyle.Bold) };
                btnOptionB = new Button { Text = q.Options[1], Location = new Point(160, 260), Size = new Size(480, 40), BackColor = Color.Yellow, Font = new Font("Arial", 10, FontStyle.Bold) };
                btnOptionC = new Button { Text = q.Options[2], Location = new Point(160, 300), Size = new Size(480, 40), BackColor = Color.Yellow, Font = new Font("Arial", 10, FontStyle.Bold) };
                btnOptionD = new Button { Text = q.Options[3], Location = new Point(160, 340), Size = new Size(480, 40), BackColor = Color.Yellow, Font = new Font("Arial", 10, FontStyle.Bold) };
                lblScore = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? $"Điểm: {score}/{multipleChoiceTopics[selectedTopic].Count}" : $"Score: {score}/{multipleChoiceTopics[selectedTopic].Count}",
                    Location = new Point(160, 600), // Move below the explanation panel
                    Size = new Size(480, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                explanationPanel = new Panel
                {
                    Location = new Point(160, 390), // Move below the option buttons
                    Size = new Size(480, 200),
                    BackColor = Color.LightGray,
                    Visible = false
                };
                Label lblExplanation = new Label
                {
                    Text = "",
                    Location = new Point(10, 10),
                    Size = new Size(460, 140),
                    Font = new Font("Arial", 10),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                explanationPanel.Controls.Add(lblExplanation);

                btnOptionA.Click += (s, e) => BtnOption_Click(s, e, q, lblExplanation);
                btnOptionB.Click += (s, e) => BtnOption_Click(s, e, q, lblExplanation);
                btnOptionC.Click += (s, e) => BtnOption_Click(s, e, q, lblExplanation);
                btnOptionD.Click += (s, e) => BtnOption_Click(s, e, q, lblExplanation);

                gamePanel.Controls.AddRange(new Control[] { lblQuestion, cardPanel, btnOptionA, btnOptionB, btnOptionC, btnOptionD, lblScore, explanationPanel });
            }
            // The rest of the ShowQuiz method remains unchanged
            else if (currentQuizType == "TrueFalse" && currentQuestionIndex < trueFalseQuestions.Count)
            {
                TrueFalseQuestion q = trueFalseQuestions[currentQuestionIndex];
                lblQuestion = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? q.NativeText : q.Text,
                    Location = new Point(160, 50), // Center horizontally
                    Size = new Size(480, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                btnTrue = new Button
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? "Đúng" : "True",
                    Location = new Point(260, 100), // Center the pair
                    Size = new Size(120, 40),
                    BackColor = Color.Green,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                btnFalse = new Button
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? "Sai" : "False",
                    Location = new Point(420, 100), // Center the pair
                    Size = new Size(120, 40),
                    BackColor = Color.Red,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                lblScore = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? $"Điểm: {score}/{trueFalseQuestions.Count}" : $"Score: {score}/{trueFalseQuestions.Count}",
                    Location = new Point(160, 150), // Center horizontally
                    Size = new Size(480, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                btnTrue.Click += BtnTrueFalse_Click;
                btnFalse.Click += BtnTrueFalse_Click;

                gamePanel.Controls.AddRange(new Control[] { lblQuestion, btnTrue, btnFalse, lblScore });
            }
            else if (currentQuizType == "FillInBlank" && currentQuestionIndex < fillInBlankQuestions.Count)
            {
                FillInBlankQuestion q = fillInBlankQuestions[currentQuestionIndex];
                lblQuestion = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? q.NativeText : q.Text,
                    Location = new Point(160, 50), // Center horizontally
                    Size = new Size(480, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                txtFillInBlankAnswer = new TextBox
                {
                    Location = new Point(160, 100), // Center horizontally
                    Size = new Size(480, 30),
                    Font = new Font("Arial", 10)
                };
                btnSubmitFillInBlank = new Button
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? "Nộp" : "Submit",
                    Location = new Point(360, 140), // Center horizontally
                    Size = new Size(80, 30),
                    BackColor = Color.Orange,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                lblScore = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? $"Điểm: {score}/{fillInBlankQuestions.Count}" : $"Score: {score}/{fillInBlankQuestions.Count}",
                    Location = new Point(160, 180), // Center horizontally
                    Size = new Size(480, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                Label lblHint = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? $"Gợi ý: {q.NativeHint}" : $"Hint: {q.Hint}",
                    Location = new Point(160, 220), // Center horizontally
                    Size = new Size(480, 30),
                    Font = new Font("Arial", 10, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                btnSubmitFillInBlank.Click += BtnSubmitFillInBlank_Click;

                gamePanel.Controls.AddRange(new Control[] { lblQuestion, txtFillInBlankAnswer, btnSubmitFillInBlank, lblScore, lblHint });
            }
            else if (currentQuizType == "Grammar" && currentQuestionIndex < grammarQuestions.Count)
            {
                GrammarQuestion q = grammarQuestions[currentQuestionIndex];
                lblQuestion = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? q.NativeText : q.Text,
                    Location = new Point(160, 50), // Center horizontally
                    Size = new Size(480, 40),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                rdoGrammarOption1 = new RadioButton { Text = q.Options[0], Location = new Point(160, 100), Size = new Size(480, 30), Font = new Font("Arial", 10) };
                rdoGrammarOption2 = new RadioButton { Text = q.Options[1], Location = new Point(160, 150), Size = new Size(480, 30), Font = new Font("Arial", 10) };
                rdoGrammarOption3 = new RadioButton { Text = q.Options[2], Location = new Point(160, 200), Size = new Size(480, 30), Font = new Font("Arial", 10) };
                rdoGrammarOption4 = new RadioButton { Text = q.Options[3], Location = new Point(160, 250), Size = new Size(480, 30), Font = new Font("Arial", 10) };
                btnSubmitGrammar = new Button
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? "Nộp" : "Submit",
                    Location = new Point(360, 300), // Center horizontally
                    Size = new Size(80, 30),
                    BackColor = Color.Orange,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                lblScore = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? $"Điểm: {score}/{grammarQuestions.Count}" : $"Score: {score}/{grammarQuestions.Count}",
                    Location = new Point(160, 340), // Center horizontally
                    Size = new Size(480, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                btnSubmitGrammar.Click += (s, e) =>
                {
                    string answer = rdoGrammarOption1.Checked ? rdoGrammarOption1.Text :
                                    rdoGrammarOption2.Checked ? rdoGrammarOption2.Text :
                                    rdoGrammarOption3.Checked ? rdoGrammarOption3.Text :
                                    rdoGrammarOption4.Checked ? rdoGrammarOption4.Text : "";
                    if (!string.IsNullOrEmpty(answer))
                    {
                        explanationPanel = new Panel
                        {
                            Location = new Point(160, 100), // Center horizontally
                            Size = new Size(480, 200),
                            BackColor = Color.LightGray,
                            Visible = true
                        };
                        Label lblResult = new Label
                        {
                            Location = new Point(10, 10),
                            Size = new Size(460, 100),
                            Font = new Font("Arial", 10),
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        Button btnNext = new Button
                        {
                            Text = selectedLanguagePair == "Vietnamese-English" ? "Tiếp tục" : "Next",
                            Location = new Point(190, 120),
                            Size = new Size(100, 30),
                            BackColor = Color.Green,
                            ForeColor = Color.White,
                            Font = new Font("Arial", 10, FontStyle.Bold)
                        };
                        btnNext.Click += (s2, e2) => { currentQuestionIndex++; ShowQuiz(); };

                        if (answer == q.CorrectAnswer)
                        {
                            score++;
                            lblResult.Text = selectedLanguagePair == "Vietnamese-English" ?
                                $"Đúng!\nGiải thích: {q.NativeExplanation}" :
                                $"Correct!\nExplanation: {q.Explanation}";
                            lblResult.ForeColor = Color.Green;
                        }
                        else
                        {
                            lblResult.Text = selectedLanguagePair == "Vietnamese-English" ?
                                $"Sai!\nĐáp án đúng: {q.CorrectAnswer}\nGiải thích: {q.NativeExplanation}" :
                                $"Wrong!\nCorrect answer: {q.CorrectAnswer}\nExplanation: {q.Explanation}";
                            lblResult.ForeColor = Color.Red;
                        }

                        explanationPanel.Controls.AddRange(new Control[] { lblResult, btnNext });
                        gamePanel.Controls.Add(explanationPanel);

                        rdoGrammarOption1.Visible = false;
                        rdoGrammarOption2.Visible = false;
                        rdoGrammarOption3.Visible = false;
                        rdoGrammarOption4.Visible = false;
                        btnSubmitGrammar.Visible = false;
                        lblScore.Text = selectedLanguagePair == "Vietnamese-English" ?
                            $"Điểm: {score}/{grammarQuestions.Count}" : $"Score: {score}/{grammarQuestions.Count}";
                    }
                    else
                    {
                        MessageBox.Show(selectedLanguagePair == "Vietnamese-English" ?
                            "Vui lòng chọn một đáp án!" : "Please select an answer!",
                            "Cảnh báo / Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                gamePanel.Controls.AddRange(new Control[] { lblQuestion, rdoGrammarOption1, rdoGrammarOption2, rdoGrammarOption3, rdoGrammarOption4, btnSubmitGrammar, lblScore });
            }
            else
            {
                Label lblResult = new Label
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ?
                        $"Hoàn thành bài kiểm tra!\nĐiểm: {score}" :
                        $"Quiz Completed!\nScore: {score}",
                    Location = new Point(160, 150), // Center horizontally
                    Size = new Size(480, 60),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 14, FontStyle.Bold)
                };
                Button btnRestart = new Button
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? "Chơi lại" : "Restart",
                    Location = new Point(280, 220), // Center the pair
                    Size = new Size(100, 40),
                    BackColor = Color.Green,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                Button btnBackToMenu = new Button
                {
                    Text = selectedLanguagePair == "Vietnamese-English" ? "Quay lại" : "Back to Menu",
                    Location = new Point(420, 220), // Center the pair
                    Size = new Size(100, 40),
                    BackColor = Color.Blue,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };

                btnRestart.Click += (s, e) => { currentQuestionIndex = 0; score = 0; ShowQuiz(); };
                btnBackToMenu.Click += (s, e) => { BtnBack_Click(s, e); };

                gamePanel.Controls.AddRange(new Control[] { lblResult, btnRestart, btnBackToMenu });
            }
        }
        private void BtnOption_Click(object sender, EventArgs e, Question q, Label lblExplanation)
        {
            Button btn = (Button)sender;
            string answer = btn.Text;

            btnOptionA.Visible = false;
            btnOptionB.Visible = false;
            btnOptionC.Visible = false;
            btnOptionD.Visible = false;

            bool isCorrect = answer == q.CorrectAnswer;
            if (isCorrect)
            {
                score++;
                btn.BackColor = Color.Green;
            }
            else
            {
                btn.BackColor = Color.Red;
                if (btnOptionA.Text == q.CorrectAnswer) btnOptionA.BackColor = Color.Green;
                else if (btnOptionB.Text == q.CorrectAnswer) btnOptionB.BackColor = Color.Green;
                else if (btnOptionC.Text == q.CorrectAnswer) btnOptionC.BackColor = Color.Green;
                else if (btnOptionD.Text == q.CorrectAnswer) btnOptionD.BackColor = Color.Green;
            }

            explanationPanel.Visible = true;
            lblExplanation.Text = selectedLanguagePair == "Vietnamese-English" ?
                (isCorrect ? $"Đúng!\n{q.NativeImageExplanation}" : $"Sai!\nĐáp án đúng: {q.CorrectAnswer}\n{q.NativeImageExplanation}") :
                (isCorrect ? $"Correct!\n{q.ImageExplanation}" : $"Wrong!\nCorrect answer: {q.CorrectAnswer}\n{q.ImageExplanation}");
            lblExplanation.ForeColor = isCorrect ? Color.Green : Color.Red;

            lblScore.Text = selectedLanguagePair == "Vietnamese-English" ?
                $"Điểm: {score}/{multipleChoiceTopics[selectedTopic].Count}" :
                $"Score: {score}/{multipleChoiceTopics[selectedTopic].Count}";

            Button btnNext = new Button
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Tiếp tục" : "Next",
                Location = new Point(190, 150), // Adjust position within the panel
                Size = new Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnNext.Click += (s, ev) => { currentQuestionIndex++; ShowQuiz(); };
            explanationPanel.Controls.Add(btnNext);
        }
        private void BtnTrueFalse_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            bool answer = (selectedLanguagePair == "Vietnamese-English" ? btn.Text == "Đúng" : btn.Text == "True");
            bool isCorrect = answer == trueFalseQuestions[currentQuestionIndex].IsTrue;

            explanationPanel = new Panel
            {
                Location = new Point(160, 100), // Center horizontally
                Size = new Size(480, 200),
                BackColor = Color.LightGray,
                Visible = true
            };
            Label lblResult = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(460, 100),
                Font = new Font("Arial", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = isCorrect ?
                    (selectedLanguagePair == "Vietnamese-English" ? "Đúng!" : "Correct!") :
                    (selectedLanguagePair == "Vietnamese-English" ? $"Sai!\nĐáp án đúng: {(trueFalseQuestions[currentQuestionIndex].IsTrue ? "Đúng" : "Sai")}" : $"Wrong!\nCorrect answer: {(trueFalseQuestions[currentQuestionIndex].IsTrue ? "True" : "False")}")
            };
            lblResult.ForeColor = isCorrect ? Color.Green : Color.Red;

            Button btnNext = new Button
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Tiếp tục" : "Next",
                Location = new Point(190, 120),
                Size = new Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnNext.Click += (s, ev) => { currentQuestionIndex++; ShowQuiz(); };

            if (isCorrect) score++;
            lblScore.Text = selectedLanguagePair == "Vietnamese-English" ?
                $"Điểm: {score}/{trueFalseQuestions.Count}" : $"Score: {score}/{trueFalseQuestions.Count}";

            btnTrue.Visible = false;
            btnFalse.Visible = false;

            explanationPanel.Controls.AddRange(new Control[] { lblResult, btnNext });
            gamePanel.Controls.Add(explanationPanel);
        }

        private void BtnSubmitFillInBlank_Click(object sender, EventArgs e)
        {
            string answer = txtFillInBlankAnswer.Text.Trim().ToLower();
            bool isCorrect = answer == fillInBlankQuestions[currentQuestionIndex].CorrectAnswer.ToLower();

            explanationPanel = new Panel
            {
                Location = new Point(160, 100), // Center horizontally
                Size = new Size(480, 200),
                BackColor = Color.LightGray,
                Visible = true
            };
            Label lblResult = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(460, 100),
                Font = new Font("Arial", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = isCorrect ?
                    (selectedLanguagePair == "Vietnamese-English" ? "Đúng!" : "Correct!") :
                    (selectedLanguagePair == "Vietnamese-English" ? $"Sai!\nĐáp án đúng: {fillInBlankQuestions[currentQuestionIndex].CorrectAnswer}" : $"Wrong!\nCorrect answer: {fillInBlankQuestions[currentQuestionIndex].CorrectAnswer}")
            };
            lblResult.ForeColor = isCorrect ? Color.Green : Color.Red;

            Button btnNext = new Button
            {
                Text = selectedLanguagePair == "Vietnamese-English" ? "Tiếp tục" : "Next",
                Location = new Point(190, 120),
                Size = new Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnNext.Click += (s, ev) => { currentQuestionIndex++; ShowQuiz(); };

            if (isCorrect) score++;
            lblScore.Text = selectedLanguagePair == "Vietnamese-English" ?
                $"Điểm: {score}/{fillInBlankQuestions.Count}" : $"Score: {score}/{fillInBlankQuestions.Count}";

            txtFillInBlankAnswer.Visible = false;
            btnSubmitFillInBlank.Visible = false;

            explanationPanel.Controls.AddRange(new Control[] { lblResult, btnNext });
            gamePanel.Controls.Add(explanationPanel);
        }
    }
}