sanity-examine-strange =
    { "" }[color=#004D77][italic]При взгляде на { GENDER($ent) ->
        [male] него
        [female] неё
        [epicene] них
       *[neuter] него
    } у вас появляется чувство тревоги...[/italic][/color]
sanity-examine-insane = [color=#004D77][italic]{ CAPITALIZE(SUBJECT($ent)) } { POSS-ADJ($ent) } лицо выглядит безумно...[/italic][/color]

sanity-medical-examine-acute-shock = [color=#004D77]{ CAPITALIZE(SUBJECT($target)) } в остром шоке: расширенные зрачки, бледная липкая кожа, напряжённая челюсть.[/color]
sanity-medical-examine-shock = [color=#004D77]{ CAPITALIZE(SUBJECT($target)) } в состоянии сильного шока: расширенные зрачки, липкая кожа.[/color]
sanity-medical-examine-nervous = [color=#004D77]{ CAPITALIZE(SUBJECT($target)) } заметно нервничает, дрожит и скрипит зубами.[/color]