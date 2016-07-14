§switch
switch($$filter$$) {§inc
	$$cases$$ §dec
}
§case
case $$case$$:§inc
	$$code$$§dec
§break
break;
§default
default:§inc
	$$code$$§dec
#####
§switch	switch($$filter$$) {§inc§n$$cases$$ §dec§n}
§case	case $$case$$:§inc§n$$code$$§dec
§multicase	§foreach($$case$$::case $$value$$:§n)§inc$$code$$
§break	break;
§default	default:§inc§n$$code$$§dec