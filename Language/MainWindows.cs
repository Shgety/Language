using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Language
{
    public partial class MainWindows : Form
    {

        public enum VariableErrorType : int
        {
            Correct = 0,
            FirstDigit = 1,
            FirstUnknownChar = 2,
            UnknownChar = 3,
            ReserverWord = 4
        }

        public enum NumericErrorType : int
        {
            Correct = 0,
            NotNumeric = 1,
            Overflow = 2
        }

        public List<string> reservedWords = new List<string> { "Конец", "Анализ", "Начало", "Синтез", "анализа", "синтеза", "sin", "cos", "abs" };

        List<string> headers;
        List<List<string>> wordsTable;
        Dictionary<string, string> variables;
        bool clearSelection = false;
        int currentRow = 0;
        int currentWord = 0;

        public MainWindows()
        {
            InitializeComponent();
        }

        private void runCode_BTN_Click(object sender, EventArgs e)
        {
            int linesCount = code_TextBox.Lines.Count();
            if (clearSelection)
            {
                code_TextBox.SelectAll();
                code_TextBox.SelectionColor = System.Drawing.Color.Black;
                code_TextBox.SelectionBackColor = System.Drawing.Color.White;
                code_TextBox.DeselectAll();
                clearSelection = false;
            }
            output_TextBox.Text = string.Empty;
            output_TextBox.ForeColor = Color.Black;
            currentRow = currentWord = 0;
            if (linesCount == 0)
            {
                MessageBox.Show("Ошибка! На вход подан пустой текст!");
                return;
            }
            else
            {
                CollectAndOrganizeData();
            }
        }

        private void CollectAndOrganizeData()
        {
            if (wordsTable != null)
                wordsTable.Clear();
            wordsTable = new List<List<string>>();
            headers = new List<string>();
            clearSelection = false;
            if (variables != null)
                variables.Clear();
            variables = new Dictionary<string, string>();
            foreach (var line in code_TextBox.Lines)
            {
                string code = line.Replace(",", " , ").Replace(":", " : ").Replace("=", " = ").Replace("\r\n", " ")
                    .Replace("*", " * ").Replace("/", " / ").Replace("^", " ^ ").Replace("-", " - ").Replace("+", " + ")
                    .Replace("(", " ( ").Replace(")", " ) ").Replace("v", " v ").Replace("~", " ~ ");
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                code = regex.Replace(code, " ");
                code = code.Trim();
                wordsTable.Add(code.Split(' ').ToList());
            }
            AnalizeLanguage();
        }

        private bool CheckFirstWord(string wordToFind)
        {
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    //currentWord++;
                    if (word != string.Empty)
                    {
                        return word.Equals(wordToFind);
                    }
                }
                if (currentRow != (wordsTable.Count - 1))
                {
                    currentRow++;
                    currentWord = 0;
                }
                else
                {
                    if (currentWord > 0)
                    currentWord--;
                }
            }
            return false;
        }

        private void OutputError(string errorText)
        {
            output_TextBox.Text = errorText + ", строка " + (currentRow + (currentWord > 0 ? 1 : 0));
            output_TextBox.ForeColor = Color.Red;
            currentRow = currentRow - (currentWord > 0 ? 0 : 1);
            int selectionStartIndex = code_TextBox.GetFirstCharIndexFromLine(currentRow);
            int selectionLength = 1;
            if (currentWord == 0)
            {
                currentWord = wordsTable[currentRow].Count - 1;
            }
            else
            {
                currentWord--;
            }
            clearSelection = true;
            if (wordsTable[currentRow][0] == string.Empty)
            {
                changeLine(code_TextBox, currentRow, " ");
                selectionStartIndex = code_TextBox.Find(" ", selectionStartIndex, RichTextBoxFinds.MatchCase);
            }
            else
            {
                for (int i = 0; i <= currentWord; i++)
                {
                    selectionStartIndex = code_TextBox.Find(wordsTable[currentRow][i], selectionStartIndex + (i == 0 ? 0 : 1), RichTextBoxFinds.MatchCase);
                }
                selectionLength = wordsTable[currentRow][currentWord].Length;
            }
            code_TextBox.Select(selectionStartIndex, selectionLength);
            code_TextBox.SelectionColor = System.Drawing.Color.Black;
            code_TextBox.SelectionBackColor = System.Drawing.Color.Red;
        }

        void changeLine(RichTextBox RTB, int line, string text)
        {
            int s1 = RTB.GetFirstCharIndexFromLine(line);
            int s2 = line < RTB.Lines.Count() - 1 ?
                      RTB.GetFirstCharIndexFromLine(line + 1) - 1 :
                      RTB.Text.Length;
            RTB.Select(s1, s2 - s1);
            RTB.SelectedText = text;
        }

        private void AnalizeLanguage()
        {
            if (!CheckFirstWord("Начало"))
            {
                currentWord++;
                OutputError("Ошибка, программа должна начинаться со слова \"Начало\""); //Если строка просмотрена, то переход на новую строку 
                                                                                           //уже был, а если мы не на 0-ом слове, значит индекс строки не менялся и нужно увеличить на 1
                return;
            }
            else
            {
                /*if (!AnalizeHeader())
                {
                    return;
                }*/
                do
                {
                    currentWord++;
                    if (!AnalizeHeader())
                    {
                        return;
                    }
                }
                while (CheckFirstWord(";"));
                if (!AnalizeOperators())
                    return;
            }
            if (CheckFirstWord("Конец"))
            {
                currentWord++;
                foreach (List<string> row in wordsTable.Skip(currentRow))
                {
                    foreach (string word in row.Skip(currentWord))
                    {
                        currentWord++;
                        if (word != string.Empty)
                        {
                            OutputError("Ошибка, после слова \"Конец\" есть текст");
                            return;
                        }
                    }
                    currentRow++;
                    currentWord = 0;
                }
                PrintData();
                return;
            }
            else
            {
                OutputError("Ошибка, программа должна завершаться словом \"Конец\"");
                return;
            }
        }

        private void PrintData()
        {
            output_TextBox.Text = "Полученные результаты вычислений";
            foreach (var key in variables.Keys)
            {
                output_TextBox.Text += Environment.NewLine + key + " = " + variables[key];
            }
            output_TextBox.ForeColor = Color.Black;
        }

        private bool AnalizeHeader()
        {
            if (CheckFirstWord("Анализ") || CheckFirstWord("Синтез"))
            {
                bool isNotDouble = false;
                bool isFirstDouble = true;
                bool isEndSeen = false;
                bool isEmpty = true;
                /*do
                {*/
                currentWord++;
                foreach (List<string> row in wordsTable.Skip(currentRow))//берет строку на которой стоим
                {
                    foreach (string word in row.Skip(currentWord))//берет слово на котором стоим из строки
                    {
                        currentWord++;//помечаем что считали его
                        if (word != string.Empty)//если не конец строки
                        {
                            isEmpty = false;
                            if (IsDouble(word) != NumericErrorType.Correct)//если число не вещественное
                            {
                                if (isFirstDouble)//если слово первое
                                {
                                    OutputError("Ошибка, ожидается вещественное число");
                                    return false;//то выходим из определения
                                }
                                else
                                {
                                    if (word == "Конец" && !isEndSeen)
                                    {
                                        isEndSeen = true;
                                        continue;
                                    }
                                    else if (word != "Конец" && word != "анализа" && word != "синтеза")
                                    {
                                        OutputError("Ошибка, ожидается вещественное число");
                                        return false;
                                    }
                                    if ((word == "синтеза" || word == "анализа") && isEndSeen)
                                    {
                                        return true;
                                    }
                                    else if (isEndSeen && word != "синтеза" && word != "анализа")
                                    {
                                        OutputError("Ошибка, после слова \"Конец\" должны стоять слова \"анализа\" или \"синтеза\"");
                                        return false;
                                    }
                                    else if ((word == "синтеза" || word == "анализа") && !isEndSeen)
                                    {
                                        OutputError("Ошибка, перед словом \"анализа\" или \"синтеза\" пропущено слово \"Конец\"");
                                        return false;
                                    }

                                }
                                isNotDouble = true;//если слово не первое то помечаем что оно не вещественное
                                break;//завершаем проверку слова
                            }
                            else
                                isFirstDouble = false;//если число вещественное то говорим что следующее не будет первым
                        }
                    }
                    if (currentWord == wordsTable[currentRow].Count)
                        isEmpty = true;
                    if (isEmpty && isEndSeen)
                        isEmpty = false;
                    if (!isEmpty)
                    {
                        if (isNotDouble)//если второе встретившее число не вещественное
                        {
                            currentWord--;//возвращаемся чтобы считать его снова и выходим из определения
                            break;
                        }
                        if ((!CheckFirstWord("анализа") || !CheckFirstWord("синтеза")) && isEndSeen)
                        {
                            currentWord++;
                            OutputError("Ошибка, после слова \"Конец\" должны стоять слова \"анализа\" или \"синтеза\"");
                            return false;
                        }
                    }
                    else
                    {
                        currentRow++;//если конец строки то переходим на следующую
                        currentWord = 0;
                    }
                }
                /*}
                while (!isNotDouble);*/
                return true;
            }
            else
            {
                OutputError("Ошибка, ожидалось либо \"Анализ\", либо \"Синтез\"");
                return false;
            }
        }

        private bool AnalizeOperators()
        {
            bool isEndNotFinded = false;
            while (isEndNotFinded = !CheckFirstWord("Конец"))
            {
                if (currentWord > 0)
                {
                    currentWord--;
                }
                if (!AnalizeOperator())
                {
                    return false;
                }
                if (currentRow == wordsTable.Count && (currentWord == wordsTable[currentRow - 1].Count || currentWord == 0))
                {
                    break;
                }
            }
            if (!isEndNotFinded && currentWord > 0)
            {
                currentWord--;
            }
            return true;
        }

        private bool AnalizeOperator()
        {
            bool firstWord = false;
            bool doubleDotsSeen = false;
            bool readRightPart = false;
            bool isEmptyAfterDoubleDots = true;
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    currentWord++;
                    if (word != string.Empty)
                    {
                        if (!firstWord)
                        {
                            firstWord = true;
                            if (word == ":")
                            {
                                OutputError("Ошибка, пропущены метки перед знаком \":\"");
                                return false;
                            }
                            else if (IsInteger(word) != NumericErrorType.Correct)
                            {
                                if (IsVariable(word) == VariableErrorType.FirstUnknownChar)
                                {
                                    OutputError("Ошибка, недопустимый символ при перечислении меток");
                                }
                                else
                                {
                                    OutputError("Ошибка, метки могут быть заданы только целыми числами");
                                }
                                return false;
                            }
                        }
                        else if (word == ":")
                        {
                            if (!doubleDotsSeen)
                            {
                                doubleDotsSeen = true;
                                readRightPart = true;
                                break;
                            }
                            else
                            {
                                OutputError("Ошибка, два знака \":\" подряд");
                                return false;
                            }
                        }
                        else if (IsVariable(word) == VariableErrorType.FirstUnknownChar)
                        {
                            OutputError("Ошибка, недопустимый символ при перечислении меток");
                            return false;
                        }
                        else if (IsInteger(word) != NumericErrorType.Correct)
                        {
                            OutputError("Ошибка, метки могут быть заданы только целыми числами");
                            return false;
                        }
                    }
                }
                if (readRightPart)
                {
                    break;
                }
                currentRow++;
                currentWord = 0;
            }
            if (!firstWord)
            {
                OutputError("Ошибка, ожидались метки или \"Конец программы\"");
                return false;
            }
            if (!doubleDotsSeen)
            {
                OutputError("Ошибка, \":\" после меток");
                return false;
            }
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    currentWord++;
                    if (word != string.Empty)
                    {
                        isEmptyAfterDoubleDots = false;
                        VariableErrorType errorType = IsVariable(word);
                        if (errorType == VariableErrorType.Correct)
                        {
                            if (!CheckFirstWord("="))
                            {
                                OutputError("Ошибка, после переменной пропущен знак \"=\"");
                                return false;
                            }
                            else
                                currentWord++;
                            string mathResult = CheckAndExecuteMath();
                            if (mathResult != "err")
                            {
                                double value = double.Parse(mathResult);
                                if (variables.ContainsKey(word))
                                {
                                    variables[word] = Convert.ToInt64(value).ToString();
                                }
                                else
                                {
                                    variables.Add(word, Convert.ToInt64(value).ToString());
                                }
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (errorType == VariableErrorType.ReserverWord)
                        {
                            OutputError("Ошибка, переменные не могут быть заданы зарезервированными словами:\r\n\"Программа\"" +
                                    "\r\n\"Метки\"\r\n\"Конец\"\r\n\"программы\"");
                            return false;
                        }
                        else if (errorType == VariableErrorType.UnknownChar)
                        {
                            OutputError("Ошибка, недопустимый символ в имени переменной");
                            return false;
                        }
                        else if (IsInteger(word) == NumericErrorType.Correct)
                        {
                            OutputError("Ошибка, только переменным могут быть присвоены значения, дано число");
                            return false;
                        }
                        else if (errorType == VariableErrorType.FirstDigit)
                        {
                            OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                            return false;
                        }
                        else if (errorType == VariableErrorType.FirstUnknownChar)
                        {
                            OutputError("Ошибка, математическое выражение содержит недопустимый символ");
                            return false;
                        }
                    }
                }
                currentRow++;
                currentWord = 0;
            }
            if (isEmptyAfterDoubleDots)
            {
                OutputError("Ошибка, после знака ожидалось математическое выражение");
                return false;
            }
            return false;
        }

        private string CheckAndExecuteMath()
        {
            List<string> words = new List<string>();
            int openBrackets = 0;
            bool canCompute = false;
            bool prevPlus = false;
            bool prevMin = false;
            bool prevMult = false;
            bool prevPov = false;
            bool prevDig = false;
            bool prevCloseBracket = false;
            bool prevOpenBracket = false;
            bool prevFunc = false;
            foreach (List<string> row in wordsTable.Skip(currentRow))
            {
                foreach (string word in row.Skip(currentWord))
                {
                    currentWord++;
                    if (word != string.Empty)
                    {
                        if (word == "Конец")
                        {
                            currentWord--;
                            canCompute = true;
                            break;
                        }
                        else if (word == "-")
                        {
                            if (prevMin || prevPlus || prevMult || prevPov)
                            {
                                OutputError("Ошибка, два знака математического действия подряд");
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = true;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                        }
                        else if (word == "+")
                        {
                            if (!prevDig && !prevCloseBracket)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, знак математического действия \"+\" после открывающейся скобки");
                                    return "err";
                                }
                                else if (prevMin || prevPlus || prevMult || prevPov)
                                {
                                    OutputError("Ошибка, два знака математического действия подряд");
                                    return "err";
                                }
                                else if (prevFunc)
                                {
                                    OutputError("Ошибка, после функции ожидается число, функция или переменная");
                                    return "err";
                                }
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = true;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                        }
                        else if (word == "*" || word == "/")
                        {
                            if (!prevDig && !prevCloseBracket)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, знак математического действия  \"" + word + "\" после открывающейся скобки");
                                    return "err";
                                }
                                else if (prevMin || prevPlus || prevMult || prevPov)
                                {
                                    OutputError("Ошибка, два знака математического действия подряд");
                                    return "err";
                                }
                                else if (prevFunc)
                                {
                                    OutputError("Ошибка, после функции ожидается число, функция или переменная");
                                    return "err";
                                }
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = true;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                        }
                        else if (word == "^" || word == "v" || word == "~")
                        {
                            if (!prevDig && !prevCloseBracket)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, знак математического действия \"" + word + "\" после открывающейся скобки");
                                    return "err";
                                }
                                else if (prevMin || prevPlus || prevMult || prevPov)
                                {
                                    OutputError("Ошибка, два знака математического действия подряд");
                                    return "err";
                                }
                                else if (prevFunc)
                                {
                                    OutputError("Ошибка, после функции ожидается число, функция или переменная");
                                    return "err";
                                }
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = true;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                        }
                        else if (word == "(")
                        {
                            openBrackets++;
                            if (openBrackets > 20)
                            {
                                OutputError("Ошибка, допустимая глубина вложенности скобок - 20");
                                return "err";
                            }
                            if (prevCloseBracket)
                            {
                                OutputError("Ошибка, между скобками пропущен знак действия");
                                return "err";
                            }
                            else if (prevDig)
                            {
                                OutputError("Ошибка, между скобкой и числом пропущен знак действия");
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = false;
                                prevOpenBracket = true;
                                prevFunc = false;
                            }
                        }
                        else if (word == ")")
                        {
                            openBrackets--;
                            if (openBrackets < 0)
                            {
                                OutputError("Ошибка, лишняя закрывающая скобка");
                                return "err";
                            }
                            if (!prevCloseBracket && !prevDig)
                            {
                                if (prevOpenBracket)
                                {
                                    OutputError("Ошибка, пустые скобки");
                                }
                                else
                                {
                                    OutputError("Ошибка, после математического действия пропущено число");
                                }
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevCloseBracket = true;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                        }
                        else if (word == "sin" || word == "cos" || word == "abs")
                        {
                            if (prevDig)
                            {
                                OutputError("Ошибка, между функцией и числом пропушен знак математического действия");
                                return "err";
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = false;
                                prevFunc = true;
                            }

                        }
                        else if (IsInteger(word) == NumericErrorType.Correct)
                        {
                            if (prevDig || prevCloseBracket)
                            {
                                currentWord--;
                                canCompute = true;
                                prevDig = true;
                                break;
                            }
                            else
                            {
                                words.Add(word);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = true;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                        }
                        else if (IsVariable(word) == VariableErrorType.Correct)
                        {
                            if (prevDig || prevCloseBracket)
                            {
                                OutputError("Ошибка, два числа подряд");
                                return "err";
                            }
                            else if (prevCloseBracket)
                            {
                                OutputError("Ошибка, пропущено действие между переменной и скобкой");
                                return "err";
                            }
                            else if (variables.ContainsKey(word))
                            {
                                words.Add(variables[word]);
                                prevMin = false;
                                prevPlus = false;
                                prevMult = false;
                                prevPov = false;
                                prevDig = true;
                                prevCloseBracket = false;
                                prevOpenBracket = false;
                                prevFunc = false;
                            }
                            else
                            {
                                OutputError("Ошибка, обращение к неинициализированной переменной");
                                return "err";
                            }
                        }
                        else if (IsVariable(word) == VariableErrorType.FirstDigit)
                        {
                            OutputError("Ошибка, имена переменных должны начинаться с буквы, дана цифра");
                            return "err";
                        }
                        else if (IsVariable(word) == VariableErrorType.UnknownChar)
                        {
                            OutputError("Ошибка, недопустимый символ в имени переменной");
                            return "err";
                        }
                        else if (IsVariable(word) == VariableErrorType.ReserverWord)
                        {
                            if (prevDig)
                            {
                                OutputError("Ошибка, два числа подряд");
                            }
                            else
                            {
                                OutputError("Ошибка, переменные не могут быть заданы зарезервированными словами:\r\n\"Программа\"" +
                                    "\r\n\"Метки\"\r\n\"Конец\"\r\n\"программы\"");
                            }
                            return "err";
                        }
                        else
                        {
                            OutputError("Ошибка, выражение содержит недопустимый символ");
                            return "err";
                        }
                    }
                }
                if (canCompute)
                {
                    break;
                }
                else
                {
                    currentRow++;
                    currentWord = 0;
                }
            }
            if (openBrackets > 0 && prevDig)
            {
                if (prevCloseBracket)
                {
                    OutputError("Ошибка, между числом и скобкой пропущен знак действия");
                }
                else
                {
                    OutputError("Ошибка, два числа подряд");
                }
                return "err";
            }
            else if (openBrackets > 0)
            {
                OutputError("Ошибка, не все скобки закрыты");
                return "err";
            }
            else if (prevMin || prevPlus || prevPov || prevMult)
            {
                OutputError("Ошибка, после знака действия должны идти скобка \"(\", переменная или целое");
                return "err";
            }
            else if (prevFunc)
            {
                OutputError("Ошибка, после функции ожидается число, функция или переменная");
                return "err";
            }
            else if (words.Count < 1)
            {
                OutputError("Ошибка, после знака \"=\" ожидалось выражение");
                return "err";
            }
            return ComputeMath(string.Join(" ", words.ToArray()));
        }

        private string ComputeMath(string math)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            math = regex.Replace(math, " ").Replace(",", ".");
            math = Regex.Replace(math,
                @"\d+(\.\d+)?", m =>
                {
                    var x = m.ToString();
                    return x.Contains(".") ? x : string.Format("{0}.0", x);
                }
            );
            math = ComputeFunctions(math).Replace(",", ".");
           math = ComputeLog(math).Replace(",", ".");
            string result = "err";
            try
            {
                result = String.Format("{0:F20}", Convert.ToDouble(new DataTable().Compute(math, "")));
            }
            catch (System.OverflowException)
            {
                OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
            }
            catch (System.DivideByZeroException)
            {
                OutputError("Возникла ошибка в процессе вычислений. Деление на ноль");
            }
            catch (System.Data.EvaluateException)
            {
                OutputError("Возникла ошибка в процессе вычислений. Полученные вычисления превысели Int64");
            }
            return result;
        }

        private string ComputeFunctions(string math)
        {
            List<string> functions = new List<string>();
            functions = math.Split(' ').ToList();
            int curWord = 0;
            int firstFunc = 0;
            int curDig = -1;
            string dig = "";
            string word;
            int i = 0;
            bool afterFunc = false;
            math = "";
            //foreach (string word in functions.Skip(curWord))
            while (curWord < functions.Count)
            {
                word = functions[curWord];
                curWord++;

                if (word == "abs" || word == "sin" || word == "cos")
                {
                    afterFunc = true;
                    firstFunc = curWord - 1;
                    dig = "";
                    foreach (string func in functions.Skip(curWord))
                    {
                        if (func == "-")
                        {
                            dig = "-" + dig;
                        }
                        else
                        if (func == "abs" || func == "sin" || func == "cos")
                        {
                            dig = "";
                        }
                        else
                        if (func != "abs" && func != "sin" && func != "cos")
                        {
                            dig += func;
                            curWord++;
                            break;
                        }
                        curWord++;
                    }
                    curDig = curWord - 1;
                    i = curDig;
                    while (firstFunc != i)
                    {

                        i--;
                        if (functions[i] == "abs")
                        {
                            dig = Convert.ToString(Math.Abs(Convert.ToDouble(dig.Replace(".", ",")))).Replace(",", ".");
                            //curWord--;
                        }
                        else
                        if (functions[i] == "sin")
                        {
                            dig = Convert.ToString(Math.Sin(Convert.ToDouble(dig.Replace(".", ",")))).Replace(",", ".");
                            //curWord--;
                        }
                        else
                        if (functions[i] == "cos")
                        {
                            dig = Convert.ToString(Math.Cos(Convert.ToDouble(dig.Replace(".", ",")))).Replace(",", ".");
                            //curWord--;
                        }
                        else
                        if (functions[i] == "-")
                        {
                            dig = Convert.ToString(-(Convert.ToDouble(dig.Replace(".", ",")))).Replace(",", ".");
                        }
                    }
                    curWord--;
                    math += dig + " ";
                    continue;
                    //functions.RemoveRange(firstFunc, curDig - firstFunc+1);
                    //functions.Insert(firstFunc, dig);

                }
                if (Regex.IsMatch(word, @"((\d+)(\.+)(\d*))$") && afterFunc)
                {
                    curWord--;
                    afterFunc = false;
                }
                if (curWord == curDig)
                {
                    curWord++;
                    //curDig--;
                    continue;
                }
                math += word + " ";


            }

            return math;
        }

        private string ComputeBrackets(string math)
        {
            while (math.Contains("("))
            {
                string beforeOpen = math.Substring(0, math.IndexOf("("));
                string afterOpen = math.Substring(math.IndexOf("(") + 1);
                if (afterOpen.IndexOf("(") < afterOpen.IndexOf(")"))
                {
                    afterOpen = ComputeBrackets(afterOpen);
                    string inBrackets = afterOpen.Substring(0, afterOpen.IndexOf(")"));
                    afterOpen = afterOpen.Substring(afterOpen.IndexOf(")") + 1);
                    inBrackets = ComputeMath(inBrackets);
                    math = beforeOpen + inBrackets + afterOpen;
                }
                else
                {
                    string inBrackets = afterOpen.Substring(0, afterOpen.IndexOf(")"));
                    afterOpen = afterOpen.Substring(afterOpen.IndexOf(")") + 1);
                    inBrackets = ComputeMath(inBrackets);
                    math = beforeOpen + inBrackets + afterOpen;
                }
            }
            return math;
        }

        private string ComputeLog(string math)
        {
            List<string> wordsList = new List<string>();
            wordsList = math.Split(' ').ToList();
            int curWord = 0;
            int firstDig = 0;
            int secondDig = 0;
            int result = 0;
            string Sign = "";
            string word;
            math = "";
            //foreach (string word in functions.Skip(curWord))
            while (curWord < wordsList.Count)
            {
                word = wordsList[curWord];
                curWord++;

                if (word == "^" || word == "v")
                {
                    firstDig = curWord - 2;
                    secondDig = curWord;
                    Sign = word;
                    if (Sign == "^")
                    {
                        int firstIntDig = (int)double.Parse(wordsList[firstDig].Replace(".0", ""));
                        int secondIntDig = (int)double.Parse(wordsList[secondDig].Replace(".0", ""));
                        result = firstIntDig ^ secondIntDig;
                    }
                    if (Sign == "v")
                    {
                        int firstIntDig = (int)double.Parse(wordsList[firstDig].Replace(".0", ""));
                        int secondIntDig = (int)double.Parse(wordsList[secondDig].Replace(".0", ""));
                        result = firstIntDig & secondIntDig;
                    }
                    wordsList[curWord - 1] = result.ToString();
                    wordsList.RemoveAt(firstDig);
                    wordsList.RemoveAt(secondDig - 1);
                }
                if (word == "~")
                {
                    int dig = (int)double.Parse(wordsList[curWord].Replace(".0", ""));
                    result = -1 * dig;
                    wordsList[curWord] = result.ToString();
                    wordsList.RemoveAt(curWord - 1);
                }
            }
            foreach (string sym in wordsList)
            {
                math += sym;
            }
            return math;
        }

        private VariableErrorType IsVariable(string value)
        {
            char a = value.ToUpper()[0];
            if (a >= '0' && a <= '9')
            {
                return VariableErrorType.FirstDigit;
            }
            else if (!((a >= 'А' && a <= 'Я') ||
                            (a >= '0' && a <= '9') ||
                            (a == 'Ё')))
            {
                return VariableErrorType.FirstUnknownChar;
            }
            foreach (char c in value.ToUpper())
            {
                if (!((c >= 'А' && c <= 'Я') ||
                            (c >= '0' && c <= '9') ||
                            (c == 'Ё')))
                {
                    return VariableErrorType.UnknownChar;
                }
            }
            if (reservedWords.Contains(value))
            {
                return VariableErrorType.ReserverWord;
            }
            return VariableErrorType.Correct;
        }

        private NumericErrorType IsDouble(string value)//является ли вещественным
        {
            if (Regex.IsMatch(value, @"((\d+)(.+)(\d*))$"))
            {
                return NumericErrorType.Correct;
            }
            else
            {
                try
                {
                    double val = double.Parse(value, new CultureInfo("en-US"));
                }
                catch (System.OverflowException)
                {
                    return NumericErrorType.Overflow;
                }
                catch (System.FormatException)
                {
                    return NumericErrorType.NotNumeric;
                }
                return NumericErrorType.NotNumeric;
            }
        }
        private NumericErrorType IsInteger(string value)//является ли целым
        {
            if (double.TryParse(value, NumberStyles.Integer, new CultureInfo("en-US"), out _))
            {
                return NumericErrorType.Correct;
            }
            else
            {
                try
                {
                    double val = double.Parse(value, new CultureInfo("en-US"));
                }
                catch (System.OverflowException)
                {
                    return NumericErrorType.Overflow;
                }
                catch (System.FormatException)
                {
                    return NumericErrorType.NotNumeric;
                }
                return NumericErrorType.NotNumeric;
            }
        }

        private void code_TextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (clearSelection)
            {
                code_TextBox.SelectAll();
                code_TextBox.SelectionColor = System.Drawing.Color.Black;
                code_TextBox.SelectionBackColor = System.Drawing.Color.White;
                code_TextBox.DeselectAll();
                clearSelection = false;
            }
        }
    }
}
