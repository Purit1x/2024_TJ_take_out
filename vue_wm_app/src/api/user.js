//导入request.js请求工具
import request from '@/utils/request.js'
import axios from 'axios'  
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api'; 


// 用户注册服务  
export const userRegisterService = async (userData) => {  
    try {  
        const response = await axios.post(`${BASE_URL}/Users/register`, userData);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
//提供调用登录接口的函数
export const userLoginService = async(loginData) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Users/login`, loginData); 
        return response.data;
    } catch (error) {  
        throw error;   
    }  
}

export const updateUser = async(data) => {

}

export const userInfo = async(id) => {

}