//导入request.js请求工具
import request from '@/utils/request.js'
import axios from 'axios'  
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api'; 

// 用户注册服务  
export const merchantRegisterService = async (merchantData) => {  
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/register`, merchantData);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
//提供调用登录接口的函数
export const merchantLoginService = async(loginData) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/login`, loginData); 
        return response.data;
    } catch (error) {  
        throw error;   
    }  
}

export const updateMerchant = async(data) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/merchantEdit`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    } 
}

export const merchantInfo = async(id) => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/merchantSearch?MerchantId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const walletRecharge=async(id,addMoney) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/recharge?merchantId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const searchDishes = async(id) => {
    try {  
        const response = await axios.get(`http://localhost:5079/api/Merchant/dishSearch?merchantId=${id}`);
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
