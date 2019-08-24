// Esse serviço faz uma série de wrappers para as funcçoes do alertifyjs, de modo
// poder usá-las elegantemente em todo programa simplemente injetando serviço


import { Injectable } from '@angular/core';
import { success } from 'node_modules/alertifyjs/build/alertify.min.js';
declare let alertify: any; // Para evitar ficar mostrando erros, uma vez que
                           // Alertify está importado nos services do arquivo angular.json
                           // e para não ter que importar cada função, como fiz acima com success
                           // só para exemplificar


// import * as alertify from 'node_modules/alertifyjs/build/alertify.min.js'; // mesmo efeito da declaração acima, só que fiz o
                                                                              // import aqui, tornando desprezível a importação na
                                                                              // seção scripts do angular.json

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {


constructor() { }


confirm(message: string, okCallBack: () => any) {
  alertify.confirm(message, function(e) {
    if (e) { // onde 'e' será o evento de clicar no ok, e okCallBack é o que acontecerá depois
      okCallBack();
    } else {} // nada a fazer se clicar cancel
  });
}

success(message: string) {
  // alertify.success(message);
  success(message);
}

error(message: string) {
  alertify.error(message);
}

warning(message: string) {
  alertify.warning(message);
}

message(message: string) {
  alertify.message(message);
}



}
