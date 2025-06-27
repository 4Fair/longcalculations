using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace BigNumberCalculator
{
    public class CalculatorForm : Form
    {
        private readonly Color MainFormColor = Color.FromArgb(112, 111, 142);
        private readonly Color ButtonColor = Color.FromArgb(173, 169, 186);
        private readonly Color TextColor = Color.FromArgb(230, 230, 250);
        private readonly Color BorderColor = Color.FromArgb(90, 89, 120);

        private readonly Font TechFont = new Font("Consolas", 12);
        private readonly Font TechFontBold = new Font("Consolas", 12, FontStyle.Bold);

        private TextBox txtNumber1;
        private TextBox txtNumber2;
        private TextBox txtResult;
        private Panel resultPanel;
        private Button btnHistory;
        private const int FormWidth = 500;
        private const int FormHeight = 510;
        private const int ControlHeight = 30;
        private const int ButtonSize = 80;
        private const int ButtonSpacing = 10;

        private List<string> calculationHistory = new List<string>();

        public CalculatorForm()
        {
            this.ClientSize = new Size(FormWidth, FormHeight);
            InitializeComponents();
            InitializeMathOperations();
            SetupFormAppearance();
        }

        private void SetupFormAppearance()
        {
            this.BackColor = MainFormColor;
            this.ForeColor = TextColor;
            this.Text = "Калькулятор дуже великих чисел  /ᐠ .ᆺ. ᐟ\\ﾉ";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeMathOperations()
        {
            KaratsubaMultiplier.Initialize(VeryLongNumber.AddStrings, VeryLongNumber.SubtractStrings);
            BigDecimalDivision.Initialize(VeryLongNumber.CompareStrings, VeryLongNumber.SubtractStrings, VeryLongNumber.AddStrings);
            BigDecimalExponentiation.Initialize(
                VeryLongNumber.AddStrings,
                VeryLongNumber.SubtractStrings,
                KaratsubaMultiplier.Multiply,
                BigDecimalDivision.DivideWithRemainder,
                VeryLongNumber.CompareStrings);
        }

        private void InitializeComponents()
        {
            CreateInputFields();
            CreateOperationButtons();
            CreateResultField();
            CreateHistoryButton();
        }

        private int CenterX(int controlWidth)
        {
            return (this.ClientSize.Width - controlWidth) / 2;
        }

        private void CreateInputFields()
        {
            int controlWidth = this.ClientSize.Width - 80;
            int currentY = 30;

            var lblNumber1 = new Label
            {
                Text = "Перше число:",
                AutoSize = true,
                Font = TechFontBold,
                Location = new Point(CenterX(120), currentY),
                ForeColor = TextColor,
                BackColor = MainFormColor
            };
            this.Controls.Add(lblNumber1);

            currentY += 25;

            txtNumber1 = new TextBox
            {
                Font = TechFont,
                Location = new Point(CenterX(controlWidth), currentY),
                Width = controlWidth,
                Height = ControlHeight,
                BackColor = ButtonColor,
                ForeColor = Color.Black
            };
            this.Controls.Add(txtNumber1);

            currentY += 50;

            var lblNumber2 = new Label
            {
                Text = "Друге число:",
                AutoSize = true,
                Font = TechFontBold,
                Location = new Point(CenterX(120), currentY),
                ForeColor = TextColor,
                BackColor = MainFormColor
            };
            this.Controls.Add(lblNumber2);

            currentY += 25;

            txtNumber2 = new TextBox
            {
                Font = TechFont,
                Location = new Point(CenterX(controlWidth), currentY),
                Width = controlWidth,
                Height = ControlHeight,
                BackColor = ButtonColor,
                ForeColor = Color.Black
            };
            this.Controls.Add(txtNumber2);
        }

        private void CreateOperationButtons()
        {
            int buttonY = 180;
            int totalButtonsWidth = 5 * ButtonSize + 4 * ButtonSpacing;
            int startX = CenterX(totalButtonsWidth);

            CreateOperationButton("+", startX, buttonY, (a, b) => a + b);
            CreateOperationButton("-", startX + ButtonSize + ButtonSpacing, buttonY, (a, b) => a - b);
            CreateOperationButton("×", startX + 2 * (ButtonSize + ButtonSpacing), buttonY, MultiplyNumbers);
            CreateOperationButton("÷", startX + 3 * (ButtonSize + ButtonSpacing), buttonY, DivideNumbers);
            CreateOperationButton("x^y", startX + 4 * (ButtonSize + ButtonSpacing), buttonY, PowerNumbers);
        }

        private void CreateHistoryButton()
        {
            int buttonY = 180 + ButtonSize + 20;
            int buttonWidth = ButtonSize * 2 + ButtonSpacing;

            btnHistory = new Button
            {
                Text = "Історія",
                Font = TechFont,
                Location = new Point(CenterX(buttonWidth), buttonY - 5),
                Size = new Size(buttonWidth, ButtonSize / 2),
                TabStop = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonColor,
                ForeColor = Color.Black
            };

            btnHistory.FlatAppearance.BorderColor = TextColor;
            btnHistory.FlatAppearance.MouseOverBackColor = Color.FromArgb(162, 158, 175);
            btnHistory.FlatAppearance.MouseDownBackColor = Color.FromArgb(142, 138, 155);

            btnHistory.Click += (sender, e) =>
            {
                ShowHistoryDialog();
            };

            this.Controls.Add(btnHistory);
        }

        private void ShowHistoryDialog()
        {
            var historyForm = new Form
            {
                Text = "Історія обчислень ≽^- ˕ -^≼",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = MainFormColor,
                ForeColor = TextColor
            };

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = TechFont,
                BackColor = ButtonColor,
                ForeColor = Color.Black
            };

            var clearButton = new Button
            {
                Text = "Очистити історію",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = TechFont,
                BackColor = ButtonColor,
                ForeColor = Color.Black
            };

            clearButton.Click += (s, e) =>
            {
                calculationHistory.Clear();
                textBox.Text = string.Empty;
            };

            historyForm.Controls.Add(textBox);
            historyForm.Controls.Add(clearButton);

            textBox.Text = string.Join(Environment.NewLine + new string('-', 50) + Environment.NewLine, calculationHistory);
            historyForm.ShowDialog(this);
        }

        private void CreateResultField()
        {
            int panelY = 320;
            int panelWidth = this.ClientSize.Width - 80;

            var lblResult = new Label
            {
                Text = "Результат:",
                AutoSize = true,
                Font = TechFontBold,
                Location = new Point(CenterX(120), panelY),
                ForeColor = TextColor,
                BackColor = MainFormColor
            };
            this.Controls.Add(lblResult);

            panelY += 25;

            resultPanel = new Panel
            {
                Location = new Point(CenterX(panelWidth), panelY),
                Size = new Size(panelWidth, 150),
                BackColor = ButtonColor,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            txtResult = new TextBox
            {
                Font = TechFont,
                Location = new Point(0, 0),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = ButtonColor,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Width = resultPanel.Width - SystemInformation.VerticalScrollBarWidth - 2,
                Height = resultPanel.Height
            };

            txtResult.TextChanged += (s, e) => {
                txtResult.ScrollBars = txtResult.Lines.Length > 10 ? ScrollBars.Vertical : ScrollBars.None;
                txtResult.Width = resultPanel.Width - (txtResult.ScrollBars == ScrollBars.Vertical ?
                    SystemInformation.VerticalScrollBarWidth : 0) - 2;
            };

            resultPanel.Controls.Add(txtResult);
            this.Controls.Add(resultPanel);
        }

        private VeryLongNumber MultiplyNumbers(VeryLongNumber a, VeryLongNumber b)
        {
            string result = KaratsubaMultiplier.Multiply(a.ToString(), b.ToString());
            return new VeryLongNumber(result);
        }

        private VeryLongNumber DivideNumbers(VeryLongNumber a, VeryLongNumber b)
        {
            var (quotient, _) = BigDecimalDivision.DivideWithRemainder(a.ToString(), b.ToString());
            return new VeryLongNumber(quotient);
        }

        private VeryLongNumber PowerNumbers(VeryLongNumber a, VeryLongNumber b)
        {
            string result = BigDecimalExponentiation.Pow(a.ToString(), b.ToString());
            return new VeryLongNumber(result);
        }

        private void CreateOperationButton(string text, int x, int y, Func<VeryLongNumber, VeryLongNumber, VeryLongNumber> operation)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font(TechFont.FontFamily, text == "x^y" ? 12 : 16),
                Location = new Point(x, y),
                Size = new Size(ButtonSize, ButtonSize),
                TabStop = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonColor,
                ForeColor = Color.Black
            };

            btn.FlatAppearance.BorderColor = TextColor;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(162, 158, 175);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(142, 138, 155);

            btn.Click += (sender, e) =>
            {
                try
                {
                    var num1 = new VeryLongNumber(txtNumber1.Text);
                    var num2 = new VeryLongNumber(txtNumber2.Text);
                    var result = operation(num1, num2);
                    txtResult.Text = result.ToString();

                    string operationSymbol = text;
                    if (text == "×") operationSymbol = "*";
                    if (text == "÷") operationSymbol = "/";
                    if (text == "x^y") operationSymbol = "^";

                    string historyEntry = $"{num1} {operationSymbol} {num2} = {result}";
                    calculationHistory.Insert(0, historyEntry);
                }
                catch (Exception ex)
                {
                    txtResult.Text = $"Помилка: {ex.Message}";
                }
            };

            this.Controls.Add(btn);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CalculatorForm());
        }
    }
}