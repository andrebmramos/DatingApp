import { Photo } from './photo';

export interface User {
    // Análogo do UserForListDto
    id: number;
    username: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: Date;
    photoUrl: string;
    city: string;
    country: string;
    // Análogo do UserForDetailedDto
    // notar que opcionais tem que ser declarados no fim
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];

}
