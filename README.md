# math-problem-via-threads
Студентам предложена для решения задача по программированию, для решения которой необходимо однократно прочитать данные из входного текстового файла, что-то посчитать и поместить результаты работы в выходной текстовый файл.

Для проверки решения этой задачи разработан набор тестов. Каждый тест состоит из входного файла (с расширением .IN) и эталонного выходного файла (с расширением .OUT). Имена входного и выходного файла совпадают для каждого теста. Весь набор тестов находится в одном каталоге.

К сожалению, входные файлы некоторых тестов содержат ошибки, связанные с несоблюдением ограничений задачи (например, в условии сказано, что число вершин графа не превосходит 100, а тест содержит большее число). В остальном структура тестового входного файла считается корректной

Вам необходимо разработать диалоговое приложение, содержащего головной тред и три подчиненных треда:
-  тред, проверяющий правильность входных данных очередного теста;
-  тред, решающий поставленную задачу;
-  тред - чекер, проверяющий правильность решенной задачи на соответствие эталонному выходному файлу. Если решение задачи неоднозначно, чекер должен проверить правильность решения, используя входной файл.

Головной тред должен:
-  прочитать в диалоге информацию о расположении набора тестов;
-  запускать одновременно не более одного экземпляра каждого подчиненного треда;
-  организовать параллельную работу подчиненных тредов, так что, например,  одновременно проверяется корректность теста № 3, задача решается на тесте № 2, чекер проверяет результаты решения теста № 1 (это называется конвейерной обработкой);
-  по требованию пользователя прекращать работу треда, решающего задачу;
-  не допускать запуска треда, решающего задачу, на неправильных тестах;
-  не допускать запуска чекера, если тред, решающий задачу, не создал выходной файл;
-  выдавать на экран результаты тестирования на всем наборе тестов в виде, удобном для человеческого восприятия.

Синхронизацию работы с совместно используемыми ресурсами должны обеспечивать подчиненные треды – для синхронизации следует использовать функции Windows API.

Варианты задачи

3. Близко расположенные вершины
4. 
Задан ориентированный граф с N вершинами и M дугами (2 ≤ N ≤ 104, 0 ≤ M ≤ N2-N). Вершины графа пронумерованы, начиная с единицы. Любая пара вершин может быть соединена не более чем одной дугой, петли не допускаются. Для каждой i-й дуги (1 ≤ i ≤ M) задана ее длина Di – целое положительное число, не превосходящее 1000. Требуется определить все пары вершин, длина кратчайшего пути между которыми не превосходит K (1 ≤ K ≤ 107).

Входные данные: текстовый файл CLOSE.IN содержит N+1 строку. Первая строка файла содержит значения N и K. Каждая из последующих N строк содержит длины всех дуг, выходящих из соответствующей вершины. Если дуги не существует, ее длина полагается равной нулю.

Выходные данные помещаются в текстовый файл CLOSE.OUT. Первая строка файла должна содержать число найденных пар вершин, а каждая из последующих строк - номера начальной и конечной вершин для каждой пары, а также длину кратчайшего пути между этими вершинами. Все строки, кроме первой, должны быть упорядочены по номерам начальной, а затем – по номерам конечной вершин. Числа в строках должны разделяться одним или несколькими пробелами.
