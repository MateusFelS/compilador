# compilador
 
## Leitura e Escrita:

		write() - Escrever valor na tela
			Exemplo: write("Olá, Mundo!")	   
     		
		read() - Ler um valor do teclado
			Exemplo: var name = read()
     
## Declaração de variáveis:

		var – pode ser atribuído valores
			Exemplo: var a = 5
                		 a = a + 1
                 
		let – somente leitura
        		Exemplo: let a = 0

## Tipos:

		int – números inteiros
			Exemplo: 1, 2, 3, 4, 5 ...
	
		String – texto (escrever entre áspas "")
			Exemplo: "Me da nota, Della Mura"
	 
		Bool – true ou false
			Exemplo: false

## Operações matemáticas:

		“+” - Adição
			Exemplo: 5 + 2
         
		“-“ – Subtração
			Exemplo: 4 - 3
	          
		“*” – Multiplicação
			Exemplo: 8 * 3
	 
		“/” – Divisão
			Exemplo: 4 / 2

## Operadores de comparação:

		">" - Maior
			Exemplo: 4 > 2
 
		">=" - Maior ou igual
			Exemplo: 1 >= 1
	 
		"<" - Menor
			Exemplo:  2 < 8
	      
		"<=" - Menor ou igual
			Exemplo: 10 <= 9
	 
		"=" - Atribuir
			Exemplo: a = 10
	 
		"==" - Igual
			Exemplo: 5 == 5
 
		"!=" - Diferente
			Exemplo: "Mateus" != "João"
	 
		"&&" - AND
			Exemplo: 10 < 20 && 50 == 50
 
		"||" - OR
			Exemplo: true || false
	 
		"()" - Parenteses
			Exemplo: 4 * 4 + (2 + 2)
	 
		{}" - Chaves
			Exemplo: {
				   var a = 5
				   var b = 10
				 }
	 
## Laços de repetição:

		while ‘condição’
			Exemplo:  {
				    var a = 0
				    while a < 5
				    a = a + 1
				  }

	 
		for ‘inicio’ to ‘fim’
			Exemplo: {
				   var result = 0
				   for i = 1 to 10
				   result = result + 1
				 }


## Controle de fluxo:

		if ‘condição’
			Exemplo: if a != 2
				    a = 20
	 
		else 
			Exemplo: {
				   var a = 5
				   var b = 0
				   if a == 10
				      b = 20
				   else
				      b = 100
				 }



## Número random:

		rnd(‘numero máximo’)
			Exemplo: rnd(100)

## Conversão de tipos:
	 	
		string para int
			Exemplo: int("4") * 4
   
		int para string
			Exemplo: str(52)
