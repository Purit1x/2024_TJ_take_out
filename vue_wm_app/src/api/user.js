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

export const updateUser = async(data) => {  //更新用户信息
    try {  
        const response = await axios.put(`${BASE_URL}/Users/userEdit`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    } 
}

export const userInfo = async(id) => {  //获取用户信息
    try {  
        const response = await axios.get(`${BASE_URL}/Users/userSearch?userId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const walletRecharge=async(id,addMoney) => {  //充值
    try {  
        const response = await axios.put(`${BASE_URL}/Users/recharge?userId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}
export const getMerchantIds = async() => {  //获取商户id
    try {  
        const response = await axios.get(`${BASE_URL}/Users/GetMerchantIds`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const getMerchantsInfo = async(id) => {  //获取商户信息
    try {  
        const response = await axios.get(`${BASE_URL}/Users/merchantsSearch?MerchantId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
export const createFavouriteMerchant=async(data) => {  //创建收藏商户
    try {  
        const response = await axios.post(`${BASE_URL}/Users/CreateFM`,data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
export const searchFavouriteMerchant=async(userId) => {  //搜索收藏商户
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getFM?userId=${userId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const deleteFavouriteMerchant=async(data) => {  //删除收藏商户
    try {  
        const response = await axios.delete(`${BASE_URL}/Users/deleteFM`,{data});  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}

// 提交地址服务
export const submitAddressService = async (addressData) => {
    try {
        const response = await axios.post(`${BASE_URL}/Users/submitAddresses`, addressData);
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}

// 获取用户地址服务
export const getAddressService = async (userId) => {
    try {
        const response = await axios.get(`${BASE_URL}/Users/getAddresses?userID=${userId}`);
        return response.data; // 返回后端返回的地址数据
    } catch (error) {
        throw error;
    }
}

// 删除地址服务
export const deleteAddressService = async (addressId) => {
    try {
        const response = await axios.delete(`${BASE_URL}/Addresses/${addressId}`);
        return response.data; // 返回删除操作的结果
    } catch (error) {
        throw error;
    }
}