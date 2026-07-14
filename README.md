<!--
SPDX-FileCopyrightText: 2017 PJB3005 <pieterjan.briers@gmail.com>
SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
SPDX-FileCopyrightText: 2019 Ivan <silvertorch5@gmail.com>
SPDX-FileCopyrightText: 2019 Silver <silvertorch5@gmail.com>
SPDX-FileCopyrightText: 2020 Injazz <43905364+Injazz@users.noreply.github.com>
SPDX-FileCopyrightText: 2020 RedlineTriad <39059512+RedlineTriad@users.noreply.github.com>
SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
SPDX-FileCopyrightText: 2023 iglov <iglov@avalon.land>
SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
SPDX-FileCopyrightText: 2024 Rares Popa <2606875+rarepops@users.noreply.github.com>
SPDX-FileCopyrightText: 2024 router <messagebus@vk.com>
SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>

SPDX-License-Identifier: AGPL-3.0-or-later
-->

<p align="center"> <img alt="Space Station 14" width="300" height="300" src="https://github.com/Gorox221/CEV-Ares14/blob/master/Resources/Textures/Logo/logo.png" /></p>

CEV-Ares14 - это неофициальный порт режима [CEV-Eris](https://github.com/discordia-space/CEV-Eris) на движок RobustToolbox, базируемой на код-базе [SS14-Xenon](https://github.com/SS14-Xenon/Xenon-Station).

Основная идея CEV-Ares - создание атмосферы тёмного киберпанка, отойдя от концепта привычной станции. Действия игры происходят на экспедиционном корабле, на борту которого располагаются различные активы корпораций, преследующие свои цели и конфликтующие между собой ради их достижения.

## Ссылки

[Дискорд сервер CEV-Ares14]() (Временно отсутствует)

## Документация

На [официальном сайте с документацией](https://docs.spacestation14.com/) имеется вся необходимая информация о контенте SS14, движке и дизайне игры, а также много информации для начинающих разработчиков.

## Контрибьютинг

Мы рады принять вклад от любого человека. Заходите в Discord, если хотите помочь. У нас есть список проблем, которые нужно решить, и любой может за них взяться. Не бойтесь просить о помощи! Только убедитесь, что ваши изменения и PRы соответствуют руководству по контрибьюту.

Любые новые механики, системы, компоненты, прототипы и прочие уникальные нововведения для сервера должны находиться в подпапке /_Ares/ папок /Resources/*/ или /Content.*/. Структура файлов и папок в подпапке /_Ares/ должна приблизительно повторять основную структуру файлов.

К каждому изменению файлов формата .cs и .yml вне папки /_Ares/ (т.е. уже существующих файлов репозитория-родителя) необходимо добавлять соответствующие комментарии Ares-Edit о вносимых изменениях, дополняя комментарий примечанием о том, что именно было изменено, желательно на английском языке.

## Сборка

1. Клонируйте этот репозиторий.
2. Запустите `RUN_THIS.py` для инициализации подмодулей и загрузки движка.
3. Скомпилируйте содержимое.

[Более подробные инструкции по сборке проекта.](https://docs.goobstation.com/en/general-development/setup.html)

## Лицензия

Содержимое, добавленное в этот репозиторий после фиксации [8270907bdc509a3fb5ecfecde8cc14e5845ede36](https://github.com/Goob-Station/Goob-Station/commit/8270907bdc509a3fb5ecfecde8cc14e5845ede36), распространяется по лицензии GNU Affero General Public License версии 3.0, если не указано иное. См. LICENSE-AGPLv3.txt. Содержимое, внесённое в этот репозиторий до фиксации [8270907bdc509a3fb5ecfecde8cc14e5845ede36](https://github.com/Goob-Station/Goob-Station/commit/8270907bdc509a3fb5ecfecde8cc14e5845ede36), лицензируется по лицензии MIT, если не указано иное. См. LICENSE.txt.

Большинство ассетов лицензировано под [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/), если не указано иное. Лицензия и авторские права на ассеты указаны в файле метаданных. [Пример](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Обратите внимание, что некоторые ассеты лицензированы под некоммерческой [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) или аналогичной некоммерческой лицензией и должны быть удалены, если вы хотите использовать этот проект в коммерческих целях.
