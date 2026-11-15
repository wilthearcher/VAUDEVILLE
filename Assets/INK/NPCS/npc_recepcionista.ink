=== start ===

Muito boa tarde, senhor Delair. Poderia me apresentar o cartão de visita, por gentileza?

* [Hã? Quem é você? Como sabe meu nome? # min_loyalty: 0 # max_loyalty: 15 # loyalty_delta: 2]
    Perdoe-me, mas fui instruído a respondê-lo apenas se o senhor estivesse em posse do cartão de seu avô. É o caso?
    -> perguntar_cartao

* [Ok?… Aqui está, e como sabe do cartão? # min_loyalty: -15 # max_loyalty: 15 # loyalty_delta: 0]
    Excelente. Fui mandado para acompanhá-lo até o salão de festas, senhor. Já estão à sua espera, vamos?
    -> END

    === perguntar_cartao ===

* [Eu sabia que havia algo a mais com aquele velho. Bom, aqui… # min_loyalty: -15 # max_loyalty: 15 # loyalty_delta: 0]
    Excelente. Já estão à sua espera, vamos?
    -> END
