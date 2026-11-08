
=== start ===

Ícaro encontra o personagem.

* [Demonstrar apoio]
    # delta=2.0
    Você demonstrou apoio.
    -> apoio

* [Discordar]
    # delta=-1.0
    Você discordou.
    -> discordancia

=== apoio ===

O personagem reage positivamente.

* [Continuar conversando]
    O diálogo continua.
    -> END

=== discordancia ===

O personagem reage negativamente.

* [Tentar se explicar]
    Ícaro tenta explicar sua decisão.
    -> END
