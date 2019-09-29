import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig, BsLocaleService } from 'ngx-bootstrap';
import { User } from '../_models/User';
import { Router } from '@angular/router';

// Para locale; Notar que defini Brasil e Rússia
import { defineLocale } from 'ngx-bootstrap/chronos';
import { ptBrLocale } from 'ngx-bootstrap/locale';
import { ruLocale } from 'ngx-bootstrap/locale';
defineLocale('ptbr', ptBrLocale);
defineLocale('ru', ruLocale);


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  user: User;

  // Usando REACTIVE FORMS, seção 12, Aula 122
  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>; // Para customizar o Datepicker, faço uma classe parcial, que permite tratar todos os elementos
                                         // obrigatórios como opcionais. Usarei esse objeto no html.


  constructor(private authService: AuthService,
              private alertify: AlertifyService,
              private fb: FormBuilder,
              private bsLocaleService: BsLocaleService,
              private router: Router) { }

  ngOnInit() {
    this.bsLocaleService.use('ptbr');
    this.createRegisterForm();
    this.bsConfig = {
      containerClass: 'theme-red'
    };


    // feito de forma anãologa usando FormBuilder na função createRegisterForm();
    // this.registerForm = new FormGroup({
    //  username: new FormControl('Initial Value', Validators.required), // Validators traz alguns validadores prontos "out of the box"
    //  password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
    //  confirmPassword: new FormControl('', Validators.required)
    // }, this.passwordMatchValidator); // Custom validator implementado abaixo para comparar senhas
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : {'mismatch': true};
    // null = ok. Inválido = retornar mapa de erros
  }

  register() {
    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
           // Jeito de clonar interessante. A interface User, por exemplo, não tem password, mas o objeto
           // terá password porque virá do registerForm; MUDEI isdso incluindo opcional password? no User
           // para ficar explícito
      this.authService.register(this.user).subscribe(() => {
          this.alertify.success('Registration successfull');
        }, error => {
          this.alertify.error(error);
        }, () => {
          // Aproveitar para fazer o login
          this.authService.login({ username: this.user.username, password: this.user.password }).subscribe(() => {
            this.router.navigate(['/members']);
          });
      });
    }
  }


  cancel() {
    this.cancelRegister.emit(false);
    console.log('cancelled');
    // this.alertify.message('Cancelled');
  }
}
