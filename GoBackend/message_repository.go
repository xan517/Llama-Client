package main

import (
    "context"
    "time"
)

// Insert a new message into the database
func InsertMessage(conversationKey, role, content string) error {
    msg := ChatMessage{
        ConversationKey: conversationKey,
        Role:           role,
        Content:        content,
        CreatedAt:      time.Now(),
    }
    return DB.Create(&msg).Error
}

// Retrieve the last N messages for a given conversation in chronological order
func GetLastNMessages(conversationKey string, n int) ([]ChatMessage, error) {
    var msgs []ChatMessage
    // We'll fetch the last n by created_at desc, then reverse them
    if err := DB.Where("conversation_key = ?", conversationKey).
        Order("created_at desc").
        Limit(n).
        Find(&msgs).Error; err != nil {
        return nil, err
    }

    // Reverse so oldest is first
    reverseMessages(msgs)
    return msgs, nil
}

// Helper function to reverse slice
func reverseMessages(msgs []ChatMessage) {
    for i, j := 0, len(msgs)-1; i < j; i, j = i+1, j-1 {
        msgs[i], msgs[j] = msgs[j], msgs[i]
    }
}
