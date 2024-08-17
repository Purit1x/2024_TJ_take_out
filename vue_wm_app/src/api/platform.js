import axios from 'axios'  
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api'; 

//提供调用登录接口的函数
export const adminLoginService = async(loginData) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Platform/login`, loginData); 
        return response.data;
    } catch (error) {  
        throw error;   
    }  
}