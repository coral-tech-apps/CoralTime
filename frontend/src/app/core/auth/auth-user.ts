import { jwtDecode, JwtPayload } from 'jwt-decode';

export class AuthUser {
	readonly accessToken: string;
	readonly expiresIn: number;
	readonly id: number;
	readonly isSso: boolean;
	readonly nickname: string;
	readonly refreshToken: string;
	readonly refreshTokenExpiration: number;
	readonly role: string[];
	readonly policies: string[];
	readonly tokenType: string;

	constructor(data, isSso: boolean) {
		this.accessToken = data.access_token;
		this.expiresIn = data.expires_in;
		this.isSso = isSso;
		this.refreshToken = data.refresh_token;
		this.tokenType = data.token_type;

    if(!data.access_token){
      return
    }
		let decodedToken = jwtDecode(data.access_token) as CustomJwtToken;
		this.id = +decodedToken.id;
		this.nickname = decodedToken.nickname;
		this.refreshTokenExpiration = new Date().getTime() + decodedToken.refreshTokenLifeTime * 1000;
		let roleName = Array.isArray(decodedToken.role) ? decodedToken.role[0] : decodedToken.role;
		this.role = roleName;
		this.policies = decodedToken.policies;
	}
}

export interface CustomJwtToken extends JwtPayload{
  id: number;
  nickname: string;
  refreshTokenLifeTime: number;
  role: string;
  policies: string[];
}
