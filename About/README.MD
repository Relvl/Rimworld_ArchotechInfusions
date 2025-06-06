# Archotech Infusions

Мод позволяет установить связь с инфраструктурой Архотека, и получить одноразовые инструкции по улучшению предметов экипировки.

Игроку становится доступно добавляение к предметам улучшений ("зачарований"), основанных на этих инструкциях.

Улучшение - это постоянное увеличение, уменьшение, или изменение определенных характеристик предметов (статов).

[Инфраструктура](#Инфраструктура)

+ [Принтер тонкой настройки](#Принтер-тонкой-настройки)
+ [Аккумулятор принтера](#Аккумулятор-принтера)
+ [Контейнер архита](#Контейнер-архита)
+ [Генератор последовательности ключа](###Генератор-последовательности-ключа)
+ [Трансивер архотека](#Трансивер-архотека)
+ [Дешифратор инструкций](#Дешифратор-инструкций)
+ [База данных инструкий](#База-данных-инструкий)
+ [Порт твердотельных накопителей](#Порт-твердотельных-накопителей)
+ [Архитовая ремонтная станция](#Архитовая-ремонтная-станция)

[Механики](#Механики)

+ [Целостность предмета и сложность инструкции](#Целостность-предмета-и-сложность-инструкции)
+ [Улучшаемые параметры снаряжения](#Улучшаемые-параметры-снаряжения)
+ [Увеличение стоимости предмета](#Увеличение-стоимости-предмета)
+ [Появление улучшенных предметов у пешек не-колонистов](#Появление-улучшенных-предметов-у-пешек-не-колонистов)

## Инфраструктура

Все строения мода должны быть соединены специальным кабелем (см. римфеллер), а аккумулятор подключен к энергосети.

### Принтер тонкой настройки

Позволяет дополнять конкретные предметы экипировки "улучшениями", которые ранее были получены от Архотека, дешифрованы и помещены в базу данных.
После применения инструкции - она удаляется из базы данных, из контейнера архитов расходуется некоторые количество запасенного архита, а из аккумулятора расходуется
пропорциональное количество энергии.

Игрок, выбрав пешку, нажимает ПКМ на принтере, и выбирает "Использовать принтер".

Пешка подбегает к принтеру, после чего открывается окно выбора предмета экипировки, среди того, что пешка носит или держит в инвентаре.

Игрок выбирает интересующий предмет. Окно закрывается. Открывается окно применения инструкции.

В окне отображен предмет, список примененных к нему ранее инструкций с возможностью удаления, указание на "целостность" предмета, список доступных в базе данных инструкций (и их
сложности, цены в архите и энергии), кнопка "Применить".

Игрок может удалить примененную ранее инструкцию, это занимает некоторе количество времени, в течение которого пешка продолжает управлять принтером. Удаление инструкции может
привести к порче предмета (но предмет таким образом не может полностью разрушиться).

Игрок может выбрать новую инструкцию, "сложность" которой не вызовет разрушение предмета, и нажать кнопку "Применить". Печать инструкции занимает некоторое количество времени, в
течение которого пешка продолжает использовать принтер.

В зависимости от сложности инструкции предмет получит повреждения. Повреждения не могут разрушить предмет, если сложность является безопасной. Если сложность инструкции превышает
доступную целостность предмета - игроку будет показано сообщение об опасной операции, где будет отображен *примерный* шанс на полное разрушение предмета. При полном разрушении
предмета никакие ресурсы или инструкции не будут компенсированы.

### Аккумулятор принтера

Для работы всей инфраструктуры мода требуется большое количество энергии, которая может быть запасена в особом аккумуляторе.
Энергию из этого аккумулятора не получится вернуть обратно в энергосеть.
При недостаточном количестве запасенной энергии принтер не сможет начать печать инструкции, дешифратор приостановит процесс расшифровки, а трансивер прервёт приём (накопленная
часть инструкции не является цельной и подлежит стиранию).

При потреблении энергии из аккумулятора, его потребление из энергосети никак не изменяется.

Игрок может установить несколько аккумуляторов в одну сеть.

### Контейнер архита

Для печати инструкции принтеру требуется архит, который может быть запасён в контейнере архита. Каждая отдельная инструкция имеет "цену" в виде количества архита, которая будет
потреблена из контейнера.

Контейнер заправляется с использованием архитовых капсул.

Интерфейс контейнера позволяет указать максимальное количество, которое пешки могут загрузить в него.

Интерфейс контейнера позволяет извлечь архит обратно в капсулы, при том, небольшая часть архита будет утеряна, а архит, который не позволяет заполнить капсулу извлечен не будет.

При демонтаже контейнера архит не будет восстановлен.

### Генератор последовательности ключа

Для запуска передачи данных Архотеком требуется создать цифровой ключ. Его можно сгенерировать в этом здании.

Генератор ключа является объектом интереса исследователя, и пешка, которой назначена работа по исследованию, будет подходить к генератору и создавать ключ.

Количество работы на создание одного ключа всегда одинаковое.

Каждый генератор может хранить 3 созданных ключа, один из которых будет автоматически загружен в трансивер при начале последовательности получения инструкции.

При работе над генерацией ключа расходуется небольшое количество энергии. При недостаточности энергии процесс будет приостановлен, а пешка пойдёт заниматься другой работой.

По-умолчанию, приоритет работы над генерацией ключа ниже, чем приоритет исследования за столом исследований.

### Трансивер архотека

Для связи с частями инфраструктуры Архотека и получения инструкций по улучшению экипировки, игроку требуется построить трансивер.

Игрок не может заранее знать, какая инструкция будет получена, Архотек отправляет случайную инструкцию.

Строение следует расоплагать в области без крыши.

Для начала работы трансиверу требуется "свободный" дешифратор, куда он сможет передать полученную инструкцию, и сгенерированный ключ.

Игрок открывает интерфейс трансивера, где он может запустить последовательность приёма инструкции однократно, либо назначить автоматический старт последовательности при наличи
ключа, свободного дешифратора и (опционально) места в базе данных.

Во время передачи зашифрованного ключа трансивер потребляет большое количество энергии из аккумулятора. Ели энергии аккумулятора не хватает для передачи части ключа - весь процесс
прервется, а ключ будет считаться скомпрометированным, и будет удалён.

После стадии передачи ключа начинается стадия приёма зашифрованной инструкции. Эта стадия потребляет меньше энергии, но при недостаточном количестве энергии в аккумуляторе процесс
так же будет безвозвратно утерян - Архотек продолжает передавать инструкцию, которую трансивер уже не принимает.

После завершения приёма инструкции, она передается на первый доступный дешифратор.

### Дешифратор инструкций

После получения зашированной инструкции трансивером, она отправится в первый доступный дешифратор, где ИИ будет расшифровывать её в применимый к принтеру формат.

Дешифратору после включения требуется некоторое время на загрузку ИИ в память и запуск программ.

Дешифраторов в одной структуре может быть несколько. Каждый дешифратор в режиме простоя потребляет небольшое количество энергии для поддержания ИИ в рабочем режиме.

Процесс дешифровки потребляет некоторое количество энергии из аккумулятора. При нехватке энергии процесс приостановится, а ИИ будет выключен. После включения ему вновь понадобится
время на загрузку, после чего процесс дешифровки продолжится.

После дешифровки инструкция будет помещена в первую доступную базу данных. Если ни в одной из баз данных недостаточно места - дешифратор поместит инструкцию в собственную
постоянную память до момента, пока освободится место в базах данных. Но до этого момента он не сможет начать дешифровку новой инструкции. Пользователю при этом отображается
сообщение.

### База данных инструкий

Для хранения расшированных инструкций перед печатью их на принтере, инструкции хранятся в базе данных.

Каждая инструкция занимает определенное количество памяти в базе, это количество памяти зависит от типа инструкции.

База данных потребляет определенное небольшое количество энергии из аккумулятора, не зависимо от наполненностью информацией.

Интерфейс базы данных позволяет удалить любую хранящуюся в ней инструкцию.

Частичное размещение инструкции в разных базах данных не предусмотрено.

### Порт твердотельных накопителей

Инструкцию из базы данных можно поместить на длительное хранение в предмет, тревдотельный накопитель.

Из накопителя инструкцию можно поместить обратно в базу данных через такой же порт.

Процесс переноса инструкции занимает некоторое время и потребляет небольшое количество энергии. При недостаточности энергии процесс прерывается, имеющиеся инструкции остаются на
своих местах. Пешка в этом процессе участвует только при загрузке пустых накопителей в порт.

Копирование инструкций не имеет смысла, они одноразовые.

Пустые твердотельные накопители можно произвести самостоятельно.

Твердотельные накопители с сохраненной инструкцией можно приобрести у фракции империи.

### Архитовая ремонтная станция

Позволяет использовать архиты для быстрого ремонта экипированного или переносимого предмета экипировки.

При использовании потребляет значительное количество энергии из аккумулятора и небольшое количество архитов из контейнера.

Игрок, выбрав пешку, нажимает ПКМ на ремонтной станции, и выбирает "Ремонтировать снаряжение".

Пешка подбежит к станции, откроется окно, в которм отображены все вещи, надетые или переносимые пешкой, с указанием прочности, расхода архитов и энергии.

Игрок выбирает предмет, начинается процесс починки. Во время починки пешка продолжает использовать станцию.

Автоматическое использование станции не предполагается.

## Механики

### Целостность предмета и сложность инструкции

Каждый предмет экипировки, на который может быть применена инструкция, имеет характеристику "Целостности", которая отражает возможность помещать новые инструкции на предмет.

Изначальная величина целостности зависит от качества предмета, от материала (?), от максимальной прочности предмета и, в случае СЕ, от брони против пронзания.

Добавление новой инструкции на предмет снижает целостность предмета, и однажды целостность станет такой, что никакую новую инструкцию на него добавить без риска разрушения уже не
получится.

Добавление новой инструкции снижает целостность предмета пропорционально сложности инструкции. Добавление новой инстркции приведет к порче предмета, пропорционально разнице между
сложностью и целостностью, но при безопасном добавлении никогда не разрушит предмет.

Целостность предмета может оказаться ниже ноля. При добавлении инструкции, сложность которой опустила бы целостность ниже ноля, пользователю будет отображено сообщение о риске
полного разрушения предмета, с указанием *примерного* на это шанса. После подтверждения такой операции будет сгенерировано случайное число, от результатов которого будет
отталкиваться успешность печати. Если печать провалилась - предмет будет полностью разрушен. Если печать была успешной - предмет будет поврежден до 1 единицы прочности.
Отрицательный уровень сложности снижает шанс на успех печати, но этот шанс никогда не может быть ниже 1%.

Сложность инструкции задается в типе инструкции. Утрированно: чем круче стат и больше значение, тем сложнее эта инструкция.

Система сбалансирована таким образом, что на условный среднестатистический предмет мидгейма отличного качества можно безопасно напечатать 3 инструкции средней сложности.

Иных способов увеличить целостность или снизить сложность не предусмотрено.

### Улучшаемые параметры снаряжения

Улучшить таким образом можно практически любой StatDef, который может быть применен к типу снаряжения.

Исключения прописываются отдельно в def файлах мода.

Диапазон величины улучшения вычисляется на основании минимальной и максимальной величины среди всех предметов такого типа, но может быть принудительно назначен для конкретного
стата или конкретной группы предметов.

Величина может быть как аддитивной, так и мультипликативной.

Величина может оказаться негативной ("вредной"), но при этом сложность такой инструкции будет считаться отрицательной, повышая целостность предмета.

### Увеличение стоимости предмета

При добавлении новых инструкций, не зависимо позитивных или негативных, стоимость предмета увеличивается не линейно. Каждая новая инструкция вносит свой множитель к общей цене за
количество инструкций. В формуле так же используется множитель за сложность инструкции - более сложные инструкции сильнее повышают цену.

### Появление улучшенных предметов у пешек не-колонистов

Любая сгенерированная пешка может с крайне низким шансом получить улучшенный предмет с 1-3 улучшенными параметрами.

Инструкции невозможно "изъять" из предмета.


## TODO
- Плоский дамаг на рендж и мили, стан