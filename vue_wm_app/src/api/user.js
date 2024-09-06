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
export const getAllUsers = async () => {
    try {
      const response = await axios.get(`${BASE_URL}/Users/getAllUsers`);
      // 这里假设后端返回的数据格式与您提供的响应体一致
      if (response.data && response.data.data) {
        // 直接返回用户列表
        return response.data.data;
      } else {
        // 如果没有用户列表，可能需要根据实际情况处理
        throw new Error('No users found');
      }
    } catch (error) {
      // 处理错误，可能是网络错误或者后端返回的错误信息
      console.error('Error fetching users:', error);
      throw error;
    }
  };
export const userInfo = async(id) => {  //获取用户信息
    try {  
        const response = await axios.get(`${BASE_URL}/Users/userSearch?userId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const deleteUser = async (userId) => {  // 删除用户
    try {
        const response = await axios.delete(`${BASE_URL}/deleteUser`, {
            params: { userId }  // 将 userId 作为参数传递
        });
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
};
export const walletRecharge=async(id,addMoney) => {  //充值
    try {  
        const response = await axios.put(`${BASE_URL}/Users/recharge?userId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const walletWithdraw=async(id,withdrawMoney) => {  //提现
    try {  
        const response = await axios.put(`${BASE_URL}/Users/withdraw?userId=${id}&withdrawMoney=${withdrawMoney}`);  
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

export const getAllMerchantsInfo = async(a,b) => {  //获取商户信息
    try {  
        const response = await axios.get(`${BASE_URL}/Users/merchantsAll?a=${a}&b=${b}`);  
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

export const getShoppingCartinMerchant=async(userId, merchantId) => {  //获取用户在某个商家处的购物车
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getShoppingCartinMerchant?userId=${userId}&merchantId=${merchantId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}

// 提交地址服务
export const submitAddressService = async (addressData) => {
    try {
        const response = await axios.post(`${BASE_URL}/Users/submitAddress`, addressData);
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
        const response = await axios.delete(`${BASE_URL}/Users/deleteAddress?addressId=${addressId}`);
        return response.data; // 返回删除操作的结果
    } catch (error) {
        throw error;
    }
}
export const EditUserAddress = async(data) => {  //编辑用户地址 
    try {  
        const response = await axios.put(`${BASE_URL}/Users/editAddress`, data);  
        return response.data; // 返回后端返回的数据  
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
export const GetDefaultAddress=async(userId) => {  //获取用户的默认地址
    try {  
        const response = await axios.get(`${BASE_URL}/Users/GetDefaultAddress?userId=${userId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const CreateDefaultAddress=async(data) => {  //设置用户的默认地址
    try {  
        const response = await axios.post(`${BASE_URL}/Users/createDefaultAddress`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const DeleteDefaultAddress=async(addressId) => {  //删除用户的默认地址
    try {  
        const response = await axios.delete(`${BASE_URL}/Users/deleteDefaultAddress?addressId=${addressId}`, )
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const GetUserAddress=async(addressId)=> {  //获取用户的地址
    try {  
        const response = await axios.get(`${BASE_URL}/Users/GetUserAddress?addressId=${addressId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const GetAddressByAddressId=async(addressId) => {  //根据地址id获取地址信息
    try {  
        const response = await axios.get(`${BASE_URL}/Users/GetAddByAddId?addressId=${addressId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const CreateOrder=async(data) => {  //创建订单
    try {  
        const response = await axios.post(`${BASE_URL}/Users/CreateOrder`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const deleteShoppingCart=async(userId,merchantId) => {  //删除购物车
    try {  
        const response = await axios.delete(`${BASE_URL}/Users/deleteShoppingCart?userId=${userId}&merchantId=${merchantId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const PurchaseOrder=async(orderId) => {  //购买订单
    try {  
        const response = await axios.post(`${BASE_URL}/Users/PurchaseOrder?orderId=${orderId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getOrders=async(userId) => {  //获取用户的订单
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getOrders?userId=${userId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getOrderCoupon=async(orderId) => {  //获取订单的优惠券
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getOrderCoupon?orderId=${orderId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getOrderDishes=async(orderId) => {  //获取订单的菜品
    try {  
        const response = await axios.get(`${BASE_URL}/Users/getOrderDishes?orderId=${orderId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const deleteOrder=async(orderId) => {  //删除订单
    try {  
        const response = await axios.delete(`${BASE_URL}/Users/deleteOrder?orderId=${orderId}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const updateOrderComment=async(data)=>{//更新订单评价
    try{
        const response =await axios.put(`${BASE_URL}/Users/updateOrderComment`,data);
        return response.data;

    }catch(error){
        throw error;
    }
}