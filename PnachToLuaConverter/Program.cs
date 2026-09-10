using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace PnachToLuaConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- PNACH to PS4 Lua Converter ---");
            Console.WriteLine("Введите путь к файлу .pnach (или перетащите файл сюда):");
            
            string inputPath = Console.ReadLine().Trim('"');

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Ошибка: Файл не найден.");
                return;
            }

            string outputPath = inputPath + ".lua";
            
            try
            {
                var luaLines = ConvertPnachToLua(inputPath);
                
                // Добавляем обертку функции, чтобы было удобно копировать в конфиг
                List<string> finalOutput = new List<string>();
                finalOutput.Add("-- Сгенерировано автоматически из PNACH");
                finalOutput.Add("local PnachMod = function()");
                finalOutput.AddRange(luaLines);
                finalOutput.Add("end");

                File.WriteAllLines(outputPath, finalOutput);

                Console.WriteLine($"\nУспешно конвертировано!");
                Console.WriteLine($"Результат сохранен в: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при конвертации: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static List<string> ConvertPnachToLua(string filePath)
        {
            var outputLines = new List<string>();
            string[] lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // 1. Обработка пустых строк
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    outputLines.Add("");
                    continue;
                }

                // 2. Обработка чистых комментариев (// -> --)
                if (trimmedLine.StartsWith("//"))
                {
                    outputLines.Add(trimmedLine.Replace("//", "--"));
                    continue;
                }

                // 3. Обработка строк патча
                // Формат: patch=1,EE,Address,Type,Value // Comment
                if (trimmedLine.StartsWith("patch="))
                {
                    try
                    {
                        // Отделяем комментарий в конце строки, если есть
                        string codePart = trimmedLine;
                        string commentPart = "";
                        int commentIndex = trimmedLine.IndexOf("//");
                        
                        if (commentIndex >= 0)
                        {
                            codePart = trimmedLine.Substring(0, commentIndex).Trim();
                            commentPart = " -- " + trimmedLine.Substring(commentIndex + 2).Trim();
                        }

                        // Разбиваем параметры патча
                        string[] parts = codePart.Split(',');

                        if (parts.Length >= 5)
                        {
                            // parts[2] = Address (hex)
                            // parts[3] = Type (word, extended, short, byte)
                            // parts[4] = Value (hex)

                            string addrStr = parts[2].Trim();
                            string type = parts[3].Trim().ToLower();
                            string valStr = parts[4].Trim();

                            // Конвертируем строку адреса в число для маскировки
                            uint address = Convert.ToUInt32(addrStr, 16);
                            
                            // ВАЖНО: PCSX2 часто использует 2XXXXXXX для записи в RAM.
                            // Эмулятор PS4 (Lua) требует чистый оффсет (0XXXXXXX).
                            // Применяем маску 0x0FFFFFFF (обнуляем первую цифру, если это 2)
                            address &= 0x0FFFFFFF;

                            uint value = Convert.ToUInt32(valStr, 16);

                            string luaCommand = "";

                            // Выбор функции в зависимости от типа
                            if (type == "word" || type == "extended")
                            {
                                // WriteMem32
                                luaCommand = $"eeObj.WriteMem32(0x{address:X8}, 0x{value:X8})";
                            }
                            else if (type == "short")
                            {
                                // WriteMem16 (на случай если попадется, хотя в примере только 32)
                                luaCommand = $"eeObj.WriteMem16(0x{address:X8}, 0x{value:X4})";
                            }
                            else if (type == "byte")
                            {
                                // WriteMem8
                                luaCommand = $"eeObj.WriteMem8(0x{address:X8}, 0x{value:X2})";
                            }
                            else
                            {
                                // Неизвестный тип, оставляем как комментарий
                                luaCommand = $"-- Неизвестный тип данных: {trimmedLine}";
                            }

                            outputLines.Add(luaCommand + commentPart);
                        }
                    }
                    catch
                    {
                        // Если парсинг сломался, сохраняем строку как комментарий с ошибкой
                        outputLines.Add($"-- Ошибка парсинга строки: {trimmedLine}");
                    }
                }
                else
                {
                    // Если строка не начинается с patch= или //, просто комментируем её
                    outputLines.Add($"-- {trimmedLine}");
                }
            }

            return outputLines;
        }
    }
}