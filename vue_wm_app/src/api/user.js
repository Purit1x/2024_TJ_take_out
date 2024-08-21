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
export const addToShoppingCart = async(shoppingCartItem) => {  //创建购物车项
    try {  
        const response = await axios.post(`${BASE_URL}/Users/addToShoppingCart`,shoppingCartItem);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const decrementDishInCart=async(shoppingCartItem) => {  //逐个删除购物车中的商品
    try {  
        const response = await axios.put(`${BASE_URL}/Users/decrementDishInCart`,shoppingCartItem);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const removeFromShoppingCart=async(shoppingCartItem) => {  //删除购物车中的商品（数量清零）
    try {  
        const response = await axios.delete(`${BASE_URL}/Users/removeFromShoppingCart`, {
            data: shoppingCartItem,  // DELETE 请求通常通过 `data` 属性来发送请求体
            headers: {
                'Content-Type': 'application/json'  // 确保 Content-Type 设置为 JSON
            }
        }); 
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getShoppingCartItems=async(userId) => {  //获取用户购物车
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getShoppingCartItems?userId=${userId}`);  
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
        const response = await axios.get(`${BASE_URL}/Users/getAddress?userId=${userId}`);
        return response.data; // 返回后端返回的地址数据
    } catch (error) {
        throw error;
    }
}

// 删除地址服务
export const deleteAddressService = async (addressId) => {
    try {
        const response = await axios.delete(`${BASE_URL}/Addresses?addressId=${addressId}`);
        return response.data; // 返回删除操作的结果
    } catch (error) {
        throw error;
    }
}
export const getAvailableCoupons=async() => {  //获取上架的优惠券
    try {  
        const response = await axios.get(`${BASE_URL}/Users/couponList`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const CreateCouponPurchase = async(data) => {  //创建优惠券购买
    try {  
        const response = await axios.post(`${BASE_URL}/Users/createCpPurchase`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getUserCoupons=async(userId) => {  //获取用户的优惠券
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getUserCoupons?userId=${userId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const GetCouponInfo=async(couponId) => {  //获取优惠券信息
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getCouponInfo?couponId=${couponId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getAllCouponPurchasesByUser=async(userId) => {  //获取用户的所有优惠券购买记录
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getAllCP?userId=${userId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}