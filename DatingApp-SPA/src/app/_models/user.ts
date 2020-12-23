import { Photo } from './photo';

export interface User {
    // Análogo do UserForListDto
    id: number;
    userName: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: any;  // Era date, mas, na hora de compilar "produção", deu erro no pipe " | timeAgo", que espera receber string. 
    photoUrl: string;
    city: string;
    country: string;
    // Análogo do UserForDetailedDto
    // notar que opcionais tem que ser declarados no fim
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
    roles?: string[];

    password?: string; // [noto que está ausente na aula 213] auxiliar para uso em funcionalidade Register (preciso fornecer senha)

}
